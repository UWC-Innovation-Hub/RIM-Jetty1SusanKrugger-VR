using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private ArtifactHighlightTrigger[] targets;

    [Header("Tutorial")]
    [SerializeField] private TutorialPopup tutorialPopup;

    private readonly HashSet<ArtifactHighlightTrigger> _completed = new HashSet<ArtifactHighlightTrigger>();

    public override void Activate()
    {
        base.Activate();

        _completed.Clear();

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"{name}: ArtifactInteractionModule has no targets configured.");
            Complete();
            return;
        }

        SubscribeToTargets();

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed += OnTutorialClosed;
            tutorialPopup.Show();
        }
        else
        {
            ArmAllTargets();
        } 
    }

    public override void Deactivate()
    {
        if (tutorialPopup != null)
        {
            tutorialPopup.Closed -= OnTutorialClosed;
        }

        UnsubscribeFromTargets();

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i]?.SetArmed(false);
            }
        }
        
        base.Deactivate();
    }

    private void OnDisable()
    {
        UnsubscribeFromTargets();
    }

    private void OnTutorialClosed()
    {
        tutorialPopup.Closed -= OnTutorialClosed;
        ArmAllTargets();
    }

    private bool OnTargetSelection(ArtifactHighlightTrigger target)
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

    private IEnumerator FinalizeAfterPlayback(ArtifactHighlightTrigger target)
    {
        target.SetArmed(false);

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
            ArtifactHighlightTrigger target = targets[i];
            if (target == null)
            {
                continue;
            }

            target.ResetTrigger();
            target.SetArmed(true);
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
