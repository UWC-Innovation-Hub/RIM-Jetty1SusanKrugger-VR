using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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

    [Header("Projection")]
    [SerializeField] private ProjectorController projector;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool muteHandAudioWhileActive = true;

    private Coroutine _sequenceRoutine;
    private readonly HashSet<HandTouchStep> _consumedSteps = new HashSet<HandTouchStep>();
    private readonly Dictionary<AudioSource, bool> _originalMuteStates = new Dictionary<AudioSource, bool>();
    private HandTouchStep _currentSelection;
    private bool _projectionFinished;
    private FieldInfo _deactivateDurationField;

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
        base.Activate();

        ResolveDependencies();
        ResolveStepDependencies();
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
                _sequenceRoutine = null;
                Complete();
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
                    _sequenceRoutine = null;
                    Complete();
                }

                yield break;
            }

            bool projectionStarted = StartProjectionForSelection(_currentSelection);
            yield return WaitForProjectionToFinish(projectionStarted);
            if (!IsActive || _currentSelection == null)
            {
                yield break;
            }

            HandTouchStep completedStep = _currentSelection;
            _currentSelection = null;
            _consumedSteps.Add(completedStep);

            HideHand(completedStep);

            if (revealDelayBetweenHands > 0f && !AllHandsConsumed())
            {
                yield return new WaitForSeconds(revealDelayBetweenHands);
            }

            ShowAvailableHands();
        }

        _sequenceRoutine = null;
    }

    private IEnumerator WaitForNextSelection()
    {
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

            yield return null;
        }
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
}
