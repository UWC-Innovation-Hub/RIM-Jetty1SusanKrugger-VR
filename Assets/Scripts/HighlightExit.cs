using UnityEngine;



//Think more carefully about implementation of grabbing and highlight material
public class HighlightExit : MonoBehaviour
{
    public PrisonerSortModule PSort;
    private Material HighLightMat;
    public HighlightExitManager HM;
    //public int identifier;
    public bool grabbed;
    public GameObject DistanceHandGrabInteractor;


    public GameObject[] Selectors;
    public Material[] RouteMats;
    private RouteHoldSelector routeHoldSelector;


    public void Start()
    {
        //HighLightMat = HM.HighLightMats[identifier];
        //Debug.Log(HighLightMat.name);
        ResolveRouteHoldSelector();
    }

    public void OnHover(int identifier)
    {
        if (ResolveRouteHoldSelector() != null)
            return;

        Debug.Log($"{name} was grabbed.");
        HighLightMat = HM.HighLightMats[identifier];
        //Highlight
        HM.HighLightMats[identifier].SetFloat("_EmissionStrength", 1f);
        //HighLightMat.SetFloat("_EmissionStrength", 2f);
        Debug.Log(HighLightMat.name);
        
    }

    public void OnDeHover(int identifier)
    {
        if (ResolveRouteHoldSelector() != null)
            return;

        Debug.Log($"{name} was released (distance).");
        Debug.Log(HighLightMat.name);
        //Highlight
        if (!grabbed)
        {
            HM.HighLightMats[identifier].SetFloat("_EmissionStrength", 0.1f);
        }
        else
        {
            DistanceHandGrabInteractor.SetActive(false);
        }        
        ////Outline
        //HighLightMat.SetFloat("_ShouldHighlight", 0f);
    }

    public void OnGrabbed()
    {
        if (ResolveRouteHoldSelector() != null)
            return;

        grabbed = true;
        Debug.Log(HighLightMat.name);
        HighLightMat.SetColor("_EmissionColor", Color.red);
        HighLightMat.SetFloat("_EmissionStrength", 1f);

        foreach(GameObject go in Selectors)
        {
            go.SetActive(false);
        }
    }


    //Where is this function called, it isn't really doing anything?
    public void ResetPaths()
    {
        if (ResolveRouteHoldSelector() != null)
        {
            routeHoldSelector.ResetSelectionState();
            return;
        }

        if (!PSort.IsComplete)
        {
            DistanceHandGrabInteractor.SetActive(true);
            grabbed = false;
            foreach (GameObject go in Selectors)
            {
                go.SetActive(true);
            }
        }

        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0.1f);
        }
    }

    private void OnApplicationQuit()
    {
        if (HighLightMat != null)
        {
            HighLightMat.SetFloat("_EmissionStrength", 0f);
            HighLightMat.SetColor("_EmissionColor", new Color(1f,0.8509f,0.2980f));
        }
        //HighLightMat.SetFloat("_ShouldHighlight", 0f);
    }

    private RouteHoldSelector ResolveRouteHoldSelector()
    {
        if (routeHoldSelector == null)
            routeHoldSelector = FindFirstObjectByType<RouteHoldSelector>();

        return routeHoldSelector;
    }
}
