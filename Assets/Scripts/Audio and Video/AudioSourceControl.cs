using UnityEngine;

public class AudioSourceControl : MonoBehaviour
{

    public AudioSource AS;
    private bool hasClosed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayAudioSource()
    {
        if(!hasClosed)
        {
            AS.Play();
        }

        hasClosed = !hasClosed;
    }
}
