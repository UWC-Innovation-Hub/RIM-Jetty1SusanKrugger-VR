using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.Video;

public class MAIO_Vid_Controller : MonoBehaviour
{
    public VideoPlayer VP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    

    public void playVid()
    {
        if (VP!=null) 
        {
            VP.Play();
        }
    }

    public void pauseVid()
    {
        if (VP != null)
        {
            VP.Pause();
        }
    }




}
