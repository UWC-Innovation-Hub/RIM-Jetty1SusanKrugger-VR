using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class FingerprintProjectionInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private FingerprintLockout lockout;

    [Header("Visibility")]
    [SerializeField] private bool hideFingerprintsWhenInactive = true;

    [Header("Light Restore")]
    [SerializeField] private Light sceneLightOverride;
    [SerializeField] private float restoredLightIntensity = 0.5f;
    [SerializeField] private float lightRestoreDuration = 1f;
    [SerializeField] private bool disableInteractionTimelineDuringLightRestore = true;

    private readonly HashSet<FingerprintTrigger> _consumedFingerprints = new HashSet<FingerprintTrigger>();
    private Coroutine _completeSelectionRoutine;
    private FingerprintTrigger _currentSelection;
    private PlayableDirector _interactionTimelineDirector;
    private Light _resolvedSceneLight;

    private FingerprintTrigger[] Fingerprints => lockout != null ? lockout.Fingerprints : null;

    private void Reset()
    {
        ResolveDependencies();
    }

    protected override void Awake()
    {
        base.Awake();
        ResolveDependencies();

        if (hideFingerprintsWhenInactive)
        {
            SetAllFingerprintsArmed(false);
            SetAllFingerprintsVisible(false);
        }
    }

    public override void Activate()
    {
        EnsureInteractionTimelineDirectorEnabled();
        base.Activate();
        ResolveDependencies();

        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null || fingerprints.Length == 0)
        {
            Debug.LogWarning($"{name}: FingerprintProjectionInteractionModule has no fingerprints configured.");
            Complete();
            return;
        }

        _consumedFingerprints.Clear();
        _currentSelection = null;

        if (_completeSelectionRoutine != null)
        {
            StopCoroutine(_completeSelectionRoutine);
            _completeSelectionRoutine = null;
        }

        SubscribeToFingerprints();
        ShowAvailableFingerprints();
    }

    public override void Deactivate()
    {
        UnsubscribeFromFingerprints();

        if (_completeSelectionRoutine != null)
        {
            StopCoroutine(_completeSelectionRoutine);
            _completeSelectionRoutine = null;
        }

        _currentSelection = null;

        if (hideFingerprintsWhenInactive)
        {
            SetAllFingerprintsArmed(false);
            SetAllFingerprintsVisible(false);
        }

        base.Deactivate();
    }

    private void OnDisable()
    {
        UnsubscribeFromFingerprints();
    }

    private bool OnFingerprintSelectionRequested(FingerprintTrigger fingerprint)
    {
        if (!IsActive || IsComplete || fingerprint == null)
        {
            return false;
        }

        if (_currentSelection != null || _consumedFingerprints.Contains(fingerprint))
        {
            return false;
        }

        AudioSource responseAudio = fingerprint.ResponseAudio;
        if (responseAudio == null || responseAudio.clip == null)
        {
            Debug.LogWarning($"{name}: Fingerprint '{fingerprint.name}' is missing its response AudioSource clip.");
            return false;
        }

        _currentSelection = fingerprint;
        HideNonSelectedFingerprints(fingerprint);

        if (_completeSelectionRoutine != null)
        {
            StopCoroutine(_completeSelectionRoutine);
        }

        _completeSelectionRoutine = StartCoroutine(FinalizeSelectionAfterAudio(responseAudio));
        return true;
    }

    private IEnumerator FinalizeSelectionAfterAudio(AudioSource responseAudio)
    {
        if (responseAudio != null)
        {
            while (IsActive && _currentSelection != null && !responseAudio.isPlaying)
            {
                yield return null;
            }

            while (IsActive && _currentSelection != null && responseAudio.isPlaying)
            {
                yield return null;
            }
        }

        if (!IsActive || _currentSelection == null)
        {
            _completeSelectionRoutine = null;
            yield break;
        }

        yield return _currentSelection.FadeOutInfoRoutine();

        if (!IsActive || _currentSelection == null)
        {
            _completeSelectionRoutine = null;
            yield break;
        }

        FingerprintTrigger completedFingerprint = _currentSelection;
        _currentSelection = null;
        _consumedFingerprints.Add(completedFingerprint);

        completedFingerprint.SetArmed(false);
        completedFingerprint.SetVisible(false);

        if (AllFingerprintsConsumed())
        {
            yield return RestoreSceneLightBeforeComplete();
            _completeSelectionRoutine = null;
            Complete();
            yield break;
        }

        ShowAvailableFingerprints();
        _completeSelectionRoutine = null;
    }

    private bool AllFingerprintsConsumed()
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null || fingerprints.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint == null)
            {
                continue;
            }

            if (!_consumedFingerprints.Contains(fingerprint))
            {
                return false;
            }
        }

        return true;
    }

    private void ShowAvailableFingerprints()
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint == null)
            {
                continue;
            }

            bool isAvailable = !_consumedFingerprints.Contains(fingerprint);
            fingerprint.SetVisible(isAvailable);
            fingerprint.SetArmed(isAvailable);
        }
    }

    private void HideNonSelectedFingerprints(FingerprintTrigger selectedFingerprint)
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint == null)
            {
                continue;
            }

            bool shouldStayVisible = fingerprint == selectedFingerprint && !_consumedFingerprints.Contains(fingerprint);
            fingerprint.SetArmed(false);
            fingerprint.SetVisible(shouldStayVisible);
        }
    }

    private void SubscribeToFingerprints()
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint == null)
            {
                continue;
            }

            fingerprint.SelectionRequested -= OnFingerprintSelectionRequested;
            fingerprint.SelectionRequested += OnFingerprintSelectionRequested;
        }
    }

    private void UnsubscribeFromFingerprints()
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint == null)
            {
                continue;
            }

            fingerprint.SelectionRequested -= OnFingerprintSelectionRequested;
        }
    }

    private void SetAllFingerprintsVisible(bool visible)
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint != null)
            {
                fingerprint.SetVisible(visible);
            }
        }
    }

    private void SetAllFingerprintsArmed(bool armed)
    {
        FingerprintTrigger[] fingerprints = Fingerprints;
        if (fingerprints == null)
        {
            return;
        }

        for (int i = 0; i < fingerprints.Length; i++)
        {
            FingerprintTrigger fingerprint = fingerprints[i];
            if (fingerprint != null)
            {
                fingerprint.SetArmed(armed);
            }
        }
    }

    private void ResolveDependencies()
    {
        if (lockout == null)
        {
            lockout = GetComponent<FingerprintLockout>();
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
