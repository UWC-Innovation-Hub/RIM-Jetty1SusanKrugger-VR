using UnityEngine;



//Think more carefully about implementation of grabbing and highlight material
public class HighlightExit : MonoBehaviour
{
    private Material HighLightMat;
    public HighlightExitManager HM;
    public int identifier;
    public bool grabbed;
    public GameObject DistanceHandGrabInteractor;


    public GameObject[] Selectors;
    public Material[] RouteMats;


    public void Start()
    {
        HighLightMat = HM.HighLightMats[identifier];
        Debug.Log(HighLightMat.name);
    }

    public void OnSelect()
    {
        Debug.Log($"{name} was grabbed.");
        //Highlight
        HighLightMat.SetFloat("_EmissionStrength", 2f);
        Debug.Log(HighLightMat.name);

        //Outline
        //HighLightMat.SetFloat("_ShouldHighlight", 1f);
    }

    public void OnDeselect()
    {
        Debug.Log($"{name} was released (distance).");
        Debug.Log(HighLightMat.name);
        //Highlight
        if (!grabbed)
        {
            HighLightMat.SetFloat("_EmissionStrength", 0f);
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
        grabbed = true;
        Debug.Log(HighLightMat.name);
        HighLightMat.SetColor("_EmissionColor", Color.red);
        HighLightMat.SetFloat("_EmissionStrength", 2f);

        foreach(GameObject go in Selectors)
        {
            go.SetActive(false);
        }


    }


    //Where is this function called, it isn't really doing anything?
    public void ResetPaths()
    {
        DistanceHandGrabInteractor.SetActive(true);
        grabbed = false;
        foreach (GameObject go in Selectors)
        {
            go.SetActive(true);
        }

        foreach (Material mat in RouteMats)
        {
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", 0f);
        }

    }

    private void OnApplicationQuit()
    {
        HighLightMat.SetFloat("_EmissionStrength", 0f);
        HighLightMat.SetColor("_EmissionColor", new Color(1f,0.8509f,0.2980f));
        //HighLightMat.SetFloat("_ShouldHighlight", 0f);
    }
}
