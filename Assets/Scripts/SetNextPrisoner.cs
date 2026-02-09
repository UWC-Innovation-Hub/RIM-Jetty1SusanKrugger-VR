using Oculus.Interaction.HandGrab;
using UnityEngine;

public class SetNextPrisoner : MonoBehaviour
{
    public ReactivateRoutes RR;

    //Increment prisoner for interaction end point detection.
    public PrisonerSortModule PrisonerSortModule;

    public void ReactivateRoute()
    {
        if (!PrisonerSortModule.IsComplete)
        {
            RR.ReactivateRoute();
        }
        //end condition for interaction
        else
        {
            RR.EndPrisonerSortInteraction();
        }
    }

    public void IncrementSorter()
    {
        RR.IncrementSorter();
    }


    //Possibly a double call here (repeated in ReactivateRoutes).
    private void OnApplicationQuit()
    {
        RR.ResetMats();
    }
}
