using Oculus.Interaction.HandGrab;
using UnityEngine;

public class SetNextPrisoner : MonoBehaviour
{
    public ReactivateRoutes RR;
    [SerializeField] private string participantId;

    //Increment prisoner for interaction end point detection.
    public PrisonerSortModule PrisonerSortModule;

    public Animator BoatAnimator;

    public void ReactivateRoute()
    {
        if (PrisonerSortModule != null && PrisonerSortModule.UsesSessionBatches)
        {
            PrisonerSortModule.RegisterParticipantFinished(ResolveParticipantId());
            return;
        }

        if (PrisonerSortModule == null)
        {
            RR?.ReactivateRoute();
            return;
        }

        if (!PrisonerSortModule.IsComplete)
        {
            RR?.ReactivateRoute();
        }
        //end condition for interaction
        else
        {
            RR?.EndPrisonerSortInteraction();
        }
    }

    public void IncrementSorter()
    {
        if (PrisonerSortModule != null && PrisonerSortModule.UsesSessionBatches)
        {
            PrisonerSortModule.RegisterParticipantArrived(ResolveParticipantId());
            BoatAnimator.SetTrigger("Close");
            return;
        }

        RR?.IncrementSorter();
    }


    //Possibly a double call here (repeated in ReactivateRoutes).
    private void OnApplicationQuit()
    {
        RR?.ResetMats();
    }

    private string ResolveParticipantId()
    {
        if (!string.IsNullOrWhiteSpace(participantId))
            return participantId;

        return gameObject.name;
    }
}
