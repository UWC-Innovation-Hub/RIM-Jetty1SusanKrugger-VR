using UnityEngine;



//Think more carefully about implementation of grabbing and highlight material
public class HighlightExit : MonoBehaviour
{
    private Material HighLightMat;
    public HighlightExitManager HM;
    public int identifier;
    private bool grabbed;


    public void Start()
    {
        HighLightMat = HM.HighLightMats[identifier];
    }


    public void OnSelect()
    {
        Debug.Log($"{name} was grabbed.");

        //HighLight
        HighLightMat.SetFloat("_EmissionStrength", 2f);


        //Outline
        //HighLightMat.SetFloat("_ShouldHighlight", 1f);

    }

    public void OnDeselect()
    {
        Debug.Log($"{name} was released (distance).");
        //Highlight
        if (!grabbed)
        {
            HighLightMat.SetFloat("_EmissionStrength", 0f);
        }
        


        ////Outline
        //HighLightMat.SetFloat("_ShouldHighlight", 0f);


    }


    public void OnGrabbed()
    {
        grabbed = true;
        HighLightMat.SetColor("_EmissionColor", Color.red);
        HighLightMat.SetFloat("_EmissionStrength", 2f);

    }


   private void OnApplicationQuit()
    {
        HighLightMat.SetFloat("_EmissionStrength", 0f);
        HighLightMat.SetColor("_EmissionColor", new Color(1f,0.8509f,0.2980f));
        //HighLightMat.SetFloat("_ShouldHighlight", 0f);
    }
}
