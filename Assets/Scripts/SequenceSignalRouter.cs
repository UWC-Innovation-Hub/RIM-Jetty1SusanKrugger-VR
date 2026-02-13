using UnityEngine;

public class SequenceSignalRouter : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private SequenceBrain brain;

    [Header("Interaction Modules")]

    ////C1
    //[SerializeField] private InteractionModuleBase InventoryInt;
    [SerializeField] private InteractionModuleBase prisonerSort;

    ////C2
    //[SerializeField] private InteractionModuleBase fingerprint;

    // Add more as needed:
    // [SerializeField] private InteractionModuleBase doorLever;
    // [SerializeField] private InteractionModuleBase dossierUI;

    private void Reset()
    {
        brain = FindFirstObjectByType<SequenceBrain>();
    }


    //// Timeline signal calls THIS at the gate moment
    //public void EnterInteraction_Inventory()
    //{
    //    if (!brain) return;
    //    brain.SetActiveInteraction(InventoryInt);
    //    brain.EnterInteraction();
    //}


    // Timeline signal calls THIS at the gate moment
    public void EnterInteraction_PrisonerSort()
    {
        if (!brain) return;
        brain.SetActiveInteraction(prisonerSort);
        brain.EnterInteraction();
    }

    //// Timeline signal calls THIS at the gate moment
    //public void EnterInteraction_Fingerprint()
    //{
    //    if (!brain) return;
    //    brain.SetActiveInteraction(fingerprint);
    //    brain.EnterInteraction();
    //}





    // Optional: if you want a generic exit signal (usually you won't)
    public void ExitInteraction_ResumeTimeline()
    {
        if (!brain) return;
        brain.ExitInteractionResumeTimeline();
    }
}
