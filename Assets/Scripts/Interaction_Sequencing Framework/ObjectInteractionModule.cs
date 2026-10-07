using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private ObjectHighlightTrigger[] targets;

    [Header("Characters")]
    [SerializeField] private CharacterFader[] charactersFadeOut;
    [SerializeField] private bool restoreCharacters = true;

    [Header("Tutorial")]
    [SerializeField] private TutorialPopup tutorialPopup;
    [SerializeField] private float tutorialTimeout = 15f;

    [Header("Highlight")]
    [Tooltip("On: targets glow as soon as the interaction starts. Off: they glow once armed.")]
    [SerializeField] private bool highlightOnActivate = true;

    private Coroutine _introRoutine;
    private Coroutine _tutorialTimeoutRoutine;
    private bool _tutorialResolved;

    private readonly HashSet<ObjectHighlightTrigger> _completed = new HashSet<ObjectHighlightTrigger>();

    public override void Activate()
    {
        base.Activate();

        _completed.Clear();
        _tutorialResolved = false;

        if (targets == null || targets.Length == 0)
        {
            Complete();
            return;
        }

        SubscribeToTargets();

        if (highlightOnActivate)
        {
            SetHighlightAll(true);
        }

        _introRoutine = StartCoroutine(IntroRoutine());
    }

    public override void Deactivate()
    {
        if (_introRoutine != null)
        {
            StopCoroutine(_introRoutine);
            _introRoutine = null;
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed -= OnTutorialClosed;
        }

        if (_tutorialTimeoutRoutine != null)
        {
            StopCoroutine(_tutorialTimeoutRoutine);
            _tutorialTimeoutRoutine = null;
        }

        UnsubscribeFromTargets();

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                {
                    continue;
                }

                targets[i].SetArmed(false);
                targets[i].SetHighlighted(false, true);
            }
        }

        if (restoreCharacters)
        {
            RestoreCharacters();
        }

        base.Deactivate();
    }

    private void OnDisable()
    {
        UnsubscribeFromTargets();
    }

    private IEnumerator IntroRoutine()
    {
        float waitTime = FadeOutCharacters();

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        _introRoutine = null;

        if (!IsActive)
        {
            yield break;
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed += OnTutorialClosed;
            tutorialPopup.Show();

            if (tutorialTimeout > 0f)
            {
                _tutorialTimeoutRoutine = StartCoroutine(TutorialTimeoutRoutine());
            }
        }
        else
        {
            ArmAllTargets();
        }
    }

    private float FadeOutCharacters()
    {
        float longest = 0f;

        if (charactersFadeOut == null)
        {
            return longest;
        }

        for (int i = 0; i < charactersFadeOut.Length; i++)
        {
            CharacterFader character = charactersFadeOut[i];

            if (character == null)
            {
                continue;
            }

            character.FadeOut();
            longest = Mathf.Max(longest, character.FadeDuration);
        }

        return longest;
    }

    private void RestoreCharacters()
    {
        if (charactersFadeOut == null)
        {
            return;
        }

        for (int i = 0; i < charactersFadeOut.Length; i++)
        {
            charactersFadeOut[i]?.SetVisibleInstant();
        }
    }

    private void OnTutorialClosed()
    {
        ResolveTutorial();
    }

    private IEnumerator TutorialTimeoutRoutine()
    {
        yield return new WaitForSeconds(tutorialTimeout);

        _tutorialTimeoutRoutine = null;

        if (_tutorialResolved)
        {
            yield break;
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.Hide();
        }

        ResolveTutorial();
    }

    private void ResolveTutorial()
    {
        if (_tutorialResolved)
        {
            return;
        }

        _tutorialResolved = true;

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed -= OnTutorialClosed;
        }

        if (_tutorialTimeoutRoutine != null)
        {
            StopCoroutine(_tutorialTimeoutRoutine);
            _tutorialTimeoutRoutine = null;
        }

        ArmAllTargets();
    }

    private bool OnTargetSelection(ObjectHighlightTrigger target)
    {
        if (!IsActive || IsComplete || target == null)
        {
            return false;
        }

        if (_completed.Contains(target))
        {
            return false;
        }

        StartCoroutine(FinalizeAfterPlayback(target));

        return true;
    }

    private IEnumerator FinalizeAfterPlayback(ObjectHighlightTrigger target)
    {
        target.SetArmed(false);
        target.SetHighlighted(false);

        float audioTime = 0f;

        if (target.ResponseAudio != null && target.ResponseAudio.clip != null)
        {
            audioTime = target.ResponseAudio.clip.length;
        }

        float videoTime = 0f;

        if (target.ResponseVideo != null && target.ResponseVideo.clip != null)
        {
            videoTime = (float)target.ResponseVideo.clip.length;
        }

        float waitTime = Mathf.Max(audioTime, videoTime);

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        if (!IsActive)
        {
            yield break;
        }

        target.HideResponseVideo();
        target.MarkComplete();
        _completed.Add(target);

        if (_completed.Count >= targets.Length)
        {
            Complete();
        }
    }

    private void ArmAllTargets()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            ObjectHighlightTrigger target = targets[i];

            if (target == null)
            {
                continue;
            }

            target.ResetTrigger();
            target.SetArmed(true);

            if (!highlightOnActivate)
            {
                target.SetHighlighted(true);
            }
        }
    }

    private void SetHighlightAll(bool highlighted)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i]?.SetHighlighted(highlighted);
        }
    }

    private void SubscribeToTargets()
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            targets[i].SelectionRequested -= OnTargetSelection;
            targets[i].SelectionRequested += OnTargetSelection;
        }
    }

    private void UnsubscribeFromTargets()
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            targets[i].SelectionRequested -= OnTargetSelection;
        }
    }
}