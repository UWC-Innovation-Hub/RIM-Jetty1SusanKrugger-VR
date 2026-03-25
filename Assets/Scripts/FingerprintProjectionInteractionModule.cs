using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerprintProjectionInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private FingerprintLockout lockout;

    [Header("Visibility")]
    [SerializeField] private bool hideFingerprintsWhenInactive = true;

    private readonly HashSet<FingerprintTrigger> _consumedFingerprints = new HashSet<FingerprintTrigger>();
    private Coroutine _completeSelectionRoutine;
    private FingerprintTrigger _currentSelection;

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
    }
}
