using UnityEngine;

public class PrisonerActorController : MonoBehaviour
{
    [Header("Animator References")]
    [SerializeField] private Animator walkAnimator;
    [SerializeField] private Animator choiceAnimator;

    private const string ShouldWalkTrigger = "ShouldWalk";
    private const string ShouldIdleTrigger = "ShouldIdle";
    private const string GoToCellTrigger = "GoToCell";
    private const string GoToBoatTrigger = "GoToBoat";
    private const string GoToTruckTrigger = "GoToTruck";

    public void Walk()
    {
        if (!walkAnimator) return;

        walkAnimator.ResetTrigger(ShouldIdleTrigger);
        walkAnimator.SetTrigger(ShouldWalkTrigger);
    }

    public void Idle()
    {
        if (!walkAnimator) return;

        walkAnimator.ResetTrigger(ShouldWalkTrigger);
        walkAnimator.SetTrigger(ShouldIdleTrigger);
    }

    public void GoToCell()
    {
        TriggerChoice(GoToCellTrigger);
    }

    public void GoToBoat()
    {
        TriggerChoice(GoToBoatTrigger);
    }

    public void GoToTruck()
    {
        TriggerChoice(GoToTruckTrigger);
    }

    public void ResetAnimators()
    {
        RebindAnimator(choiceAnimator);
        RebindAnimator(walkAnimator);
    }

    private void TriggerChoice(string triggerName)
    {
        if (!choiceAnimator) return;

        choiceAnimator.ResetTrigger(GoToCellTrigger);
        choiceAnimator.ResetTrigger(GoToBoatTrigger);
        choiceAnimator.ResetTrigger(GoToTruckTrigger);
        choiceAnimator.SetTrigger(triggerName);
    }

    private static void RebindAnimator(Animator animator)
    {
        if (!animator) return;

        animator.Rebind();
        animator.Update(0f);
    }
}
