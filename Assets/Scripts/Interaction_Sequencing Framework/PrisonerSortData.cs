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
}

[System.Serializable]
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
