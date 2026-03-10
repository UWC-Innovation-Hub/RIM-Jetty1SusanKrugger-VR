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

    public override void Activate()
    {
        base.Activate();

        ResetHands();

        if (handSteps.Count == 0)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule has no hand steps configured.");
            Complete();
            return;
        }

        _sequenceRoutine = StartCoroutine(RunSequence());
    }

    public override void Deactivate()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        if (hideHandsWhenInactive)
        {
            HideAllHands();
        }

        base.Deactivate();
    }

    private IEnumerator RunSequence()
    {
        for (int i = 0; i < handSteps.Count; i++)
        {
            HandTouchStep step = handSteps[i];
            RevealHand(step);

            yield return WaitForHandAudio(step);

            if (!IsActive)
            {
                yield break;
            }

            if (i < handSteps.Count - 1 && revealDelayBetweenHands > 0f)
            {
                yield return new WaitForSeconds(revealDelayBetweenHands);
            }
        }

        _sequenceRoutine = null;
        Complete();
    }

    private IEnumerator WaitForHandAudio(HandTouchStep step)
    {
        if (step == null || step.testimonyAudio == null)
        {
            Debug.LogWarning($"{name}: HandTouchInteractionModule step is missing its testimony AudioSource.");
            yield break;
        }

        while (IsActive && !step.testimonyAudio.isPlaying)
        {
            yield return null;
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
            HandTouchStep step = handSteps[i];
            if (step == null)
            {
                continue;
            }

            ResetFaders(step);

            if (step.handRoot != null)
            {
                step.handRoot.SetActive(false);
            }
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
