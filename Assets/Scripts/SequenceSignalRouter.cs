using UnityEngine;

public class SequenceSignalRouter : MonoBehaviour
{
    [SerializeField] private SequenceBrain brain;

    private void Reset()
    {
        brain = FindFirstObjectByType<SequenceBrain>();
    }

    public void EnterInteraction()
    {
        if (brain) brain.EnterInteraction();
    }

    public void ExitInteractionResumeTimeline()
    {
        if (brain) brain.ExitInteractionResumeTimeline();
    }
}
