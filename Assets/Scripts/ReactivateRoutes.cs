using UnityEditor.SpeedTree.Importer;
using UnityEngine;

public class ReactivateRoutes : MonoBehaviour
{
    public GameObject[] Prisoners;
    public GameObject[] Routes;
    public Material[] RouteMats;
    public HighlightExit Grabbed;
    public Animator[] PrisonerWalkAnimator;
    public GameObject DistanceHandGrabInteractor;
    private int identifier;



    //Increment prisoner for interaction end point detection.
    public PrisonerSortModule PrisonerSortModule;


    private void Awake()
    {
        identifier = 0;
    }


    public void ReactivateRoute()
    {
        foreach(GameObject go in Routes)
        {
            if (!go.activeSelf)
            {
                go.SetActive(true);
            }
        }

        DistanceHandGrabInteractor.SetActive(true);


        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0f);
        }


        //Grabbed is a single variable now that HighlightExit is a singleton referred to by each distancegrabbable
        Grabbed.grabbed = false;
        //foreach(HighlightExit dest in GrabbedAr)
        //{
        //    dest.grabbed = false;
        //}



        ////Trigger idle animation
        ////Is this needed? As once the prisoner has walked, they should deactivate?
        //PrisonerWalkAnimator[identifier].SetTrigger("ShouldIdle");


        //Deactivate Current Prisoner
        Prisoners[identifier].SetActive(false);
        identifier++;
    }


    public void EndPrisonerSortInteraction()
    {
        foreach (GameObject go in Routes)
        {
            if (!go.activeSelf)
            {
                go.SetActive(false);
            }
        }

        DistanceHandGrabInteractor.SetActive(false);

        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0f);
        }

        Grabbed.grabbed = false;


        foreach(GameObject go in Prisoners)
        {
            go.SetActive(false);
        }
        //Trigger idle animation
        //PrisonerWalkAnimator.SetTrigger("ShouldIdle");

    }



    //function to increment prisoner sort condition
    public void IncrementSorter()
    {
        PrisonerSortModule.RegisterPrisonerArrived();
    }


    private void OnApplicationQuit()
    {
        ResetMats();
    }


    public void ResetMats()
    {
        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0f);
        }
    }

}
