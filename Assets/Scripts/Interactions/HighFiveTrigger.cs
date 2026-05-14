using UnityEngine;
using UnityEngine.Video;

public class HighFiveTrigger : MonoBehaviour
{
    public PalmGestureDetector detector;
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!detector.IsOpenPalm) return;

        if (other.CompareTag("VideoWall"))
        {
            ShowVideo();
        }
    }

    void ShowVideo()
    {
        Debug.Log("Collided with Hand!");
        //videoPanel.SetActive(true);
        //videoPlayer.Play();
    }
}
