using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

public class HandTouchInteractionModule : InteractionModuleBase
{
    [System.Serializable]
    private class HandTouchStep
    {
        public GameObject handRoot;
        public AudioSource testimonyAudio;
        public MaterialOpacityFader[] revealFaders;
        public Collider selectionCollider;
        public VideoClip projectionClip;
    }

    [Header("Hand Sequence")]
    [SerializeField] private List<HandTouchStep> handSteps = new List<HandTouchStep>();
    [SerializeField] private bool hideHandsWhenInactive = true;
    [SerializeField] private float revealDelayBetweenHands = 0f;

    [Header("Per-Item Inactivity Assistance")]
    [Tooltip("Automatically select one remaining hand after the player has been inactive for the configured threshold.")]
    [SerializeField] private bool enableItemInactivityAssist = false;
    [Tooltip("How long the remaining hands can stay selectable before one is selected automatically.")]
    [Min(0f)]
    [SerializeField] private float itemInactivityTimeoutSeconds = 30f;

    [Header("Projection")]
    [SerializeField] private ProjectorController projector;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool muteHandAudioWhileActive = true;

    [Header("Recording Bypass")]
    [SerializeField] private bool autoPlayProjectionOnActivate = false;
    [SerializeField, Min(0)] private int autoPlayStepIndex = 0;
    [SerializeField] private bool autoCompleteAfterAutoProjection = false;

    [Header("Light Restore")]
    [SerializeField] private Light sceneLightOverride;
    [SerializeField] private float restoredLightIntensity = 0.5f;
    [SerializeField] private float lightRestoreDuration = 1f;
    [SerializeField] private bool disableInteractionTimelineDuringLightRestore = true;

    private Coroutine _sequenceRoutine;
    private readonly HashSet<HandTouchStep> _consumedSteps = new HashSet<HandTouchStep>();
    private readonly Dictionary<AudioSource, bool> _originalMuteStates = new Dictionary<AudioSource, bool>();
    private HandTouchStep _currentSelection;
    private bool _projectionFinished;
    private FieldInfo _deactivateDurationField;
    private PlayableDirector _interactionTimelineDirector;
    private Light _resolvedSceneLight;

    private void Reset()
    {
        ResolveDependencies();
        ResolveStepDependencies();
    }

    protected override void Awake()
    {
        base.Awake();
        ResolveDependencies();
        ResolveStepDependencies();
    }

    public override void Activate()
    {
        EnsureInteractionTimelineDirectorEnabled();
        base.Activate();

        ResolveDependencies();
        ResolveStepDependencies();

        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        RestoreHandAudioMuteStates();
        ResetHands();
        _consumedSteps.Clear();
        _currentSelection = null;
        _projectionFinished = false;

        if (handSteps.Count == 0)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule has no hand steps configured.");
            Complete();
            return;
        }

        if (muteHandAudioWhileActive)
        {
            SetHandAudioMuted(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
            videoPlayer.loopPointReached += OnVideoLoopPointReached;
        }

        if (autoPlayProjectionOnActivate)
        {
            HideAllHands();
            _sequenceRoutine = StartCoroutine(RunAutoProjectionBypass());
            return;
        }

        ShowAvailableHands();
        _sequenceRoutine = StartCoroutine(RunSelectionLoop());
    }

    public override void Deactivate()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        _currentSelection = null;
        _projectionFinished = false;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        }

        RestoreHandAudioMuteStates();

        if (hideHandsWhenInactive)
        {
            HideAllHands();
        }

        base.Deactivate();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        }

        RestoreHandAudioMuteStates();
    }

    private IEnumerator RunSelectionLoop()
    {
        while (IsActive)
        {
            if (AllHandsConsumed())
            {
                yield return RestoreSceneLightBeforeComplete();
                _sequenceRoutine = null;

                if (IsActive)
                {
                    Complete();
                }

                yield break;
            }

            yield return WaitForNextSelection();
            if (!IsActive)
            {
                yield break;
            }

            if (_currentSelection == null)
            {
                if (AllHandsConsumed())
                {
                    yield return RestoreSceneLightBeforeComplete();
                    _sequenceRoutine = null;

                    if (IsActive)
                    {
                        Complete();
                    }
                }

                yield break;
            }

            bool projectionStarted = StartProjectionForSelection(_currentSelection);
            if (!projectionStarted)
            {
                HandTouchStep failedStep = _currentSelection;
                _currentSelection = null;

                if (!IsStepStructurallyValidForProjection(failedStep, logWarning: false))
                {
                    _consumedSteps.Add(failedStep);
                    HideHand(failedStep);
                }
                else
                {
                    RearmStepSelection(failedStep);
                    ShowAvailableHands();
                }

                continue;
            }

            yield return WaitForProjectionToFinish(projectionStarted);
            if (!IsActive || _currentSelection == null)
            {
                yield break;
            }

            HandTouchStep completedStep = _currentSelection;
            _currentSelection = null;
            _consumedSteps.Add(completedStep);

            HideHand(completedStep);

            if (AllHandsConsumed())
            {
                yield return RestoreSceneLightBeforeComplete();
                _sequenceRoutine = null;
                Complete();
                yield break;
            }

            if (revealDelayBetweenHands > 0f && !AllHandsConsumed())
            {
                yield return new WaitForSeconds(revealDelayBetweenHands);
            }

            ShowAvailableHands();
        }

        _sequenceRoutine = null;
    }

    private IEnumerator RunAutoProjectionBypass()
    {
        int stepIndex = Mathf.Clamp(autoPlayStepIndex, 0, handSteps.Count - 1);
        HandTouchStep step = handSteps[stepIndex];

        _currentSelection = step;
        StopStepAudio(step);
        HideAllHands();

        bool projectionStarted = StartProjectionForSelection(step);
        yield return WaitForProjectionToFinish(projectionStarted);

        if (!IsActive)
        {
            _sequenceRoutine = null;
            yield break;
        }

        _currentSelection = null;

        if (autoCompleteAfterAutoProjection)
        {
            yield return RestoreSceneLightBeforeComplete();

            if (IsActive)
            {
                Complete();
            }
        }

        _sequenceRoutine = null;
    }

    private IEnumerator WaitForNextSelection()
    {
        float elapsed = 0f;

        while (IsActive)
        {
            for (int i = 0; i < handSteps.Count; i++)
            {
                HandTouchStep step = handSteps[i];
                if (!IsStepAvailable(step))
                {
                    continue;
                }

                if (!WasStepSelected(step))
                {
                    continue;
                }

                _currentSelection = step;
                StopStepAudio(step);
                HideNonSelectedHands(step);
                yield break;
            }

            if (AllHandsConsumed())
            {
                yield break;
            }

            if (enableItemInactivityAssist && elapsed >= itemInactivityTimeoutSeconds)
            {
                if (TrySelectNextHandForInactivityAssist())
                {
                    yield break;
                }

                if (AllHandsConsumed())
                {
                    yield break;
                }

                elapsed = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private bool TrySelectNextHandForInactivityAssist()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (!IsStepAvailable(step))
            {
                continue;
            }

            if (!IsStepStructurallyValidForProjection(step, logWarning: true))
            {
                _consumedSteps.Add(step);
                HideHand(step);
                continue;
            }

            _currentSelection = step;
            StopStepAudio(step);

            if (step.selectionCollider != null)
            {
                step.selectionCollider.enabled = false;
            }

            HideNonSelectedHands(step);
            Debug.Log($"[{name}] No hand was selected for {itemInactivityTimeoutSeconds:0.##} seconds. Selecting '{GetStepName(step)}' automatically.");
            return true;
        }

        return false;
    }

    private IEnumerator WaitForProjectionToFinish(bool projectionStarted)
    {
        if (!projectionStarted)
        {
            yield break;
        }

        while (IsActive && !_projectionFinished)
        {
            yield return null;
        }

        if (!IsActive)
        {
            yield break;
        }

        float closeDelay = GetProjectorDeactivateDuration();
        if (closeDelay > 0f)
        {
            yield return new WaitForSeconds(closeDelay);
        }
    }

    private void ResetHands()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (step == null)
            {
                continue;
            }

            if (step.testimonyAudio != null)
            {
                step.testimonyAudio.Stop();
            }

            if (step.selectionCollider != null)
            {
                step.selectionCollider.enabled = true;
            }

            StopAllStepAudio(step);
            ResetFaders(step);

            if (step.handRoot != null)
            {
                step.handRoot.SetActive(false);
            }
        }
    }

    private void HideAllHands()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HideHand(handSteps[i]);
        }
    }

    private void ShowAvailableHands()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (!IsStepAvailable(step))
            {
                continue;
            }

            RevealHand(step);
        }
    }

    private void HideNonSelectedHands(HandTouchStep selectedStep)
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (step == null || step == selectedStep || _consumedSteps.Contains(step))
            {
                continue;
            }

            HideHand(step);
        }
    }

    private void HideHand(HandTouchStep step)
    {
        if (step == null)
        {
            return;
        }

        ResetFaders(step);

        if (step.handRoot != null)
        {
            step.handRoot.SetActive(false);
        }
    }

    private bool AllHandsConsumed()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (IsStepAvailable(step))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsStepAvailable(HandTouchStep step)
    {
        return step != null && !_consumedSteps.Contains(step);
    }

    private bool WasStepSelected(HandTouchStep step)
    {
        if (step == null)
        {
            return false;
        }

        if (step.selectionCollider != null)
        {
            return !step.selectionCollider.enabled;
        }

        return step.testimonyAudio != null && step.testimonyAudio.isPlaying;
    }

    private bool StartProjectionForSelection(HandTouchStep step)
    {
        _projectionFinished = false;

        if (step == null)
        {
            return false;
        }

        if (projector == null || videoPlayer == null)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule requires a ProjectorController and VideoPlayer.");
            return false;
        }

        if (step.projectionClip == null)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule step '{GetStepName(step)}' is missing its projection clip.");
            return false;
        }

        if (!projector.TryPlay(step.projectionClip))
        {
            Debug.LogWarning($"{name}: Projector was busy when hand step '{GetStepName(step)}' was selected.");
            return false;
        }

        return true;
    }

    private bool IsStepStructurallyValidForProjection(HandTouchStep step, bool logWarning)
    {
        if (step == null)
        {
            return false;
        }

        if (projector == null || videoPlayer == null)
        {
            if (logWarning)
            {
                Debug.LogWarning($"{name}: Skipping timed hand '{GetStepName(step)}' because the projector is not configured.");
            }

            return false;
        }

        if (step.projectionClip == null)
        {
            if (logWarning)
            {
                Debug.LogWarning($"{name}: Skipping timed hand '{GetStepName(step)}' because its projection clip is not configured.");
            }

            return false;
        }

        return true;
    }

    private static void RearmStepSelection(HandTouchStep step)
    {
        if (step?.selectionCollider != null)
        {
            step.selectionCollider.enabled = true;
        }
    }

    private void OnVideoLoopPointReached(VideoPlayer player)
    {
        if (!IsActive || _currentSelection == null)
        {
            return;
        }

        _projectionFinished = true;
    }

    private float GetProjectorDeactivateDuration()
    {
        if (projector == null)
        {
            return 0f;
        }

        if (_deactivateDurationField == null)
        {
            _deactivateDurationField = typeof(ProjectorController).GetField("deactivateDuration", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if (_deactivateDurationField == null)
        {
            return 0f;
        }

        object value = _deactivateDurationField.GetValue(projector);
        if (value is float duration)
        {
            return Mathf.Max(0f, duration);
        }

        return 0f;
    }

    private void SetHandAudioMuted(bool muted)
    {
        _originalMuteStates.Clear();

        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (step?.handRoot == null)
            {
                continue;
            }

            AudioSource[] audioSources = step.handRoot.GetComponentsInChildren<AudioSource>(true);
            for (int j = 0; j < audioSources.Length; j++)
            {
                AudioSource audioSource = audioSources[j];
                if (audioSource == null || _originalMuteStates.ContainsKey(audioSource))
                {
                    continue;
                }

                _originalMuteStates[audioSource] = audioSource.mute;
                audioSource.mute = muted;
            }
        }
    }

    private void RestoreHandAudioMuteStates()
    {
        if (_originalMuteStates.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<AudioSource, bool> entry in _originalMuteStates)
        {
            if (entry.Key != null)
            {
                entry.Key.mute = entry.Value;
            }
        }

        _originalMuteStates.Clear();
    }

    private static void StopStepAudio(HandTouchStep step)
    {
        if (step?.testimonyAudio != null && step.testimonyAudio.isPlaying)
        {
            step.testimonyAudio.Stop();
        }
    }

    private static void StopAllStepAudio(HandTouchStep step)
    {
        if (step?.handRoot == null)
        {
            return;
        }

        AudioSource[] audioSources = step.handRoot.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null && audioSources[i].isPlaying)
            {
                audioSources[i].Stop();
            }
        }
    }

    private string GetStepName(HandTouchStep step)
    {
        return step?.handRoot != null ? step.handRoot.name : "(missing hand)";
    }

    private void ResolveDependencies()
    {
        if (projector == null)
        {
            projector = FindFirstObjectByType<ProjectorController>();
        }

        if (videoPlayer == null && projector != null)
        {
            videoPlayer = projector.GetComponentInChildren<VideoPlayer>(true);
        }

        if (_interactionTimelineDirector == null)
        {
            _interactionTimelineDirector = GetComponentInChildren<PlayableDirector>(true);
        }

        if (_resolvedSceneLight == null)
        {
            _resolvedSceneLight = ResolveSceneLight();
        }
    }

    private void ResolveStepDependencies()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            if (step?.handRoot == null || step.selectionCollider != null)
            {
                continue;
            }

            step.selectionCollider = step.handRoot.GetComponent<Collider>();
        }
    }

    private static void ResetFaders(HandTouchStep step)
    {
        if (step == null || step.revealFaders == null)
        {
            return;
        }

        for (int i = 0; i < step.revealFaders.Length; i++)
        {
            MaterialOpacityFader fader = step.revealFaders[i];
            if (fader != null)
            {
                fader.HideInstant();
            }
        }
    }

    private static void RevealHand(HandTouchStep step)
    {
        if (step == null)
        {
            return;
        }

        if (step.handRoot != null)
        {
            step.handRoot.SetActive(true);
        }

        if (step.revealFaders == null)
        {
            return;
        }

        for (int i = 0; i < step.revealFaders.Length; i++)
        {
            MaterialOpacityFader fader = step.revealFaders[i];
            if (fader != null)
            {
                fader.BeginFadeIn();
            }
        }
    }

    private IEnumerator RestoreSceneLightBeforeComplete()
    {
        Light targetLight = ResolveSceneLight();
        if (targetLight == null)
        {
            yield break;
        }

        if (disableInteractionTimelineDuringLightRestore)
        {
            DisableInteractionTimelineIfBoundTo(targetLight);
        }

        if (lightRestoreDuration <= 0f)
        {
            targetLight.intensity = restoredLightIntensity;
            yield break;
        }

        float startIntensity = targetLight.intensity;
        if (Mathf.Approximately(startIntensity, restoredLightIntensity))
        {
            targetLight.intensity = restoredLightIntensity;
            yield break;
        }

        float elapsed = 0f;
        while (IsActive && elapsed < lightRestoreDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lightRestoreDuration);
            float eased = t * t * (3f - 2f * t);
            targetLight.intensity = Mathf.Lerp(startIntensity, restoredLightIntensity, eased);
            yield return null;
        }

        targetLight.intensity = restoredLightIntensity;
    }

    private void EnsureInteractionTimelineDirectorEnabled()
    {
        if (_interactionTimelineDirector == null)
        {
            _interactionTimelineDirector = GetComponentInChildren<PlayableDirector>(true);
        }

        if (_interactionTimelineDirector != null && !_interactionTimelineDirector.enabled)
        {
            _interactionTimelineDirector.enabled = true;
        }
    }

    private void DisableInteractionTimelineIfBoundTo(Light targetLight)
    {
        if (targetLight == null)
        {
            return;
        }

        if (_interactionTimelineDirector == null)
        {
            _interactionTimelineDirector = GetComponentInChildren<PlayableDirector>(true);
        }

        if (_interactionTimelineDirector == null || !_interactionTimelineDirector.enabled)
        {
            return;
        }

        Light boundLight = TryResolveLightFromTimelineBinding(_interactionTimelineDirector);
        if (boundLight == targetLight)
        {
            _interactionTimelineDirector.enabled = false;
        }
    }

    private Light ResolveSceneLight()
    {
        if (sceneLightOverride != null)
        {
            return sceneLightOverride;
        }

        if (_resolvedSceneLight != null)
        {
            return _resolvedSceneLight;
        }

        if (_interactionTimelineDirector == null)
        {
            _interactionTimelineDirector = GetComponentInChildren<PlayableDirector>(true);
        }

        _resolvedSceneLight = TryResolveLightFromTimelineBinding(_interactionTimelineDirector);
        return _resolvedSceneLight;
    }

    private static Light TryResolveLightFromTimelineBinding(PlayableDirector director)
    {
        if (director == null)
        {
            return null;
        }

        if (!(director.playableAsset is TimelineAsset timelineAsset))
        {
            return null;
        }

        foreach (TrackAsset track in timelineAsset.GetOutputTracks())
        {
            Object binding = director.GetGenericBinding(track);
            Light boundLight = TryGetLightFromBinding(binding);
            if (boundLight != null)
            {
                return boundLight;
            }
        }

        return null;
    }

    private static Light TryGetLightFromBinding(Object binding)
    {
        switch (binding)
        {
            case Light light:
                return light;
            case Animator animator:
                return animator.GetComponent<Light>();
            case GameObject gameObject:
                return gameObject.GetComponent<Light>();
            case Component component:
                return component.GetComponent<Light>();
            default:
                return null;
        }
    }
}
