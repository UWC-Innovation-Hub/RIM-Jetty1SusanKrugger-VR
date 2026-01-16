using UnityEditor.SpeedTree.Importer;
using UnityEngine;

public class ReactivateRoutes : MonoBehaviour
{
    public GameObject[] Routes;
    public Material[] RouteMats;
    public HighlightExit[] GrabbedAr;
    public Animator PrisonerWalkAnimator;
    public GameObject DistanceHandGrabInteractor;


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

        foreach(HighlightExit dest in GrabbedAr)
        {
            dest.grabbed = false;
        }

        //Trigger idle animation
        PrisonerWalkAnimator.SetTrigger("ShouldIdle");
    }


    private void OnApplicationQuit()
    {
        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0f);
        }
    }
}
