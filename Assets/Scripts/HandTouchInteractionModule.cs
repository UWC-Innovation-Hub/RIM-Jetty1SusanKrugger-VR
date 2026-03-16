using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTouchInteractionModule : InteractionModuleBase
{
    [System.Serializable]
    private class HandTouchStep
    {
        public GameObject handRoot;
        public AudioSource testimonyAudio;
        public MaterialOpacityFader[] revealFaders;
    }

    [Header("Hand Sequence")]
    [SerializeField] private List<HandTouchStep> handSteps = new List<HandTouchStep>();
    [SerializeField] private bool hideHandsWhenInactive = true;
    [SerializeField] private float revealDelayBetweenHands = 0f;

    private Coroutine _sequenceRoutine;
    private readonly HashSet<HandTouchStep> _consumedSteps = new HashSet<HandTouchStep>();
    private HandTouchStep _currentSelection;

    public override void Activate()
    {
        base.Activate();

        ResetHands();
        _consumedSteps.Clear();
        _currentSelection = null;

        if (handSteps.Count == 0)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule has no hand steps configured.");
            Complete();
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

        if (hideHandsWhenInactive)
        {
            HideAllHands();
        }

        base.Deactivate();
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

            yield return WaitForHandResponseToFinish(_currentSelection);
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

                if (step.testimonyAudio == null)
                {
                    Debug.LogWarning($"{name}: HandTouchInteractionModule step is missing its testimony AudioSource.");
                    _consumedSteps.Add(step);
                    HideHand(step);
                    continue;
                }

                if (!step.testimonyAudio.isPlaying)
                {
                    continue;
                }

                _currentSelection = step;
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

    private IEnumerator WaitForHandResponseToFinish(HandTouchStep step)
    {
        if (step == null || step.testimonyAudio == null)
        {
            yield break;
        }

        while (IsActive && step.testimonyAudio.isPlaying)
        {
            yield return null;
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
