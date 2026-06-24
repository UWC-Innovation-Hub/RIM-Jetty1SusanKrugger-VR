using UnityEngine;

[System.Serializable]
public class PrisonerSortParticipant
{
    public string participantId;
    public GameObject root;
    public Animator choiceAnimator;
    public Animator walkAnimator;
    public SetNextPrisoner arrivalNotifier;

    public string ResolveParticipantId()
    {
        if (!string.IsNullOrWhiteSpace(participantId))
            return participantId;

        if (root != null)
            return root.name;

        return string.Empty;
    }

    public void SetActive(bool active)
    {
        if (root != null)
            root.SetActive(active);
    }

    public void ResetAnimators()
    {
        ResetAnimator(choiceAnimator);
        ResetAnimator(walkAnimator);
    }

    public void SendToCell()
    {
        if (choiceAnimator != null)
            choiceAnimator.SetTrigger("GoToCell");

        if (walkAnimator != null)
            walkAnimator.SetTrigger("ShouldWalk");
    }

    private static void ResetAnimator(Animator animator)
    {
        if (animator == null)
            return;

        animator.Rebind();
        animator.Update(0f);
    }
}

[System.Serializable]
// Retained only so older scenes/scripts compile while C1 uses the single-batch module.
public class PrisonerSortBatch
{
    public string batchId;
    public PrisonerSortParticipant[] participants;
    public GameObject[] linkedObjects;
    [Min(0)] public int requiredArrivals;

    public int GetRequiredArrivals()
    {
        if (requiredArrivals > 0)
            return requiredArrivals;

        return participants != null ? participants.Length : 0;
    }
}

[System.Serializable]
// Retained only so older scenes/scripts compile while C1 uses the single-batch module.
public class PrisonerSortSession
{
    public string sessionId;
    public PrisonerSortBatch[] batches;

    public int GetBatchCount()
    {
        return batches != null ? batches.Length : 0;
    }

    public PrisonerSortBatch GetBatch(int index)
    {
        if (batches == null || index < 0 || index >= batches.Length)
            return null;

        return batches[index];
    }
}
