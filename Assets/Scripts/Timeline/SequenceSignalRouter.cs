using UnityEngine;

public class SequenceSignalRouter : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private SequenceBrain brain;

    [Header("Interaction Modules")]
    [SerializeField] private InteractionModuleBase[] interactionModules;

    private void Reset()
    {
        brain = FindFirstObjectByType<SequenceBrain>();
    }

    // Timeline signal calls this at the gate moment.
    public void EnterInteractionByIndex(int moduleIndex)
    {
        if (!brain) return;

        if (interactionModules == null || moduleIndex < 0 || moduleIndex >= interactionModules.Length)
        {
            Debug.LogWarning($"{name}: Invalid interaction module index {moduleIndex}.");
            return;
        }

        InteractionModuleBase module = interactionModules[moduleIndex];
        if (!module)
        {
            Debug.LogWarning($"{name}: Interaction module at index {moduleIndex} is not assigned.");
            return;
        }

        brain.SetActiveInteraction(module);
        brain.EnterInteraction();
    }

    // Optional: if you want a generic exit signal (usually you won't)
    public void ExitInteraction_ResumeTimeline()
    {
        if (!brain) return;
        brain.ExitInteractionResumeTimeline();
    }
}
