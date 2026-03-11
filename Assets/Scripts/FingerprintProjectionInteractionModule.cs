using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;

public class FingerprintProjectionInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private FingerprintLockout lockout;
    [SerializeField] private ProjectorController projector;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Visibility")]
    [SerializeField] private bool hideFingerprintsWhenInactive = true;

    private readonly HashSet<FingerprintTrigger> _consumedFingerprints = new HashSet<FingerprintTrigger>();
    private Coroutine _completeSelectionRoutine;
    private FingerprintTrigger _currentSelection;
    private FieldInfo _deactivateDurationField;

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
        base.Activate();
        ResolveDependencies();

        if (projector == null || videoPlayer == null)
        {
            Debug.LogWarning($"{name}: FingerprintProjectionInteractionModule requires a ProjectorController and VideoPlayer.");
            Complete();
            return;
        }

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

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        videoPlayer.loopPointReached += OnVideoLoopPointReached;

        SubscribeToFingerprints();
        ShowAvailableFingerprints();
    }

    public override void Deactivate()
    {
        UnsubscribeFromFingerprints();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        }

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

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        }
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

        if (projector == null || fingerprint.Clip == null)
        {
            return false;
        }

        if (!projector.TryPlay(fingerprint.Clip))
        {
            return false;
        }

        _currentSelection = fingerprint;
        HideNonSelectedFingerprints(fingerprint);
        return true;
    }

    private void OnVideoLoopPointReached(VideoPlayer player)
    {
        if (!IsActive || _currentSelection == null)
        {
            return;
        }

        if (_completeSelectionRoutine != null)
        {
            StopCoroutine(_completeSelectionRoutine);
        }

        _completeSelectionRoutine = StartCoroutine(FinalizeSelectionAfterProjectorClose());
    }

    private IEnumerator FinalizeSelectionAfterProjectorClose()
    {
        float closeDelay = GetProjectorDeactivateDuration();
        if (closeDelay > 0f)
        {
            yield return new WaitForSeconds(closeDelay);
        }

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

    private void ResolveDependencies()
    {
        if (lockout == null)
        {
            lockout = GetComponent<FingerprintLockout>();
        }

        if (projector == null)
        {
            projector = FindFirstObjectByType<ProjectorController>();
        }

        if (videoPlayer == null && projector != null)
        {
            videoPlayer = projector.GetComponentInChildren<VideoPlayer>(true);
        }
    }
}
