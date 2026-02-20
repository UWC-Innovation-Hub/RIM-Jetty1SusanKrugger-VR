using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VideoTrigger : MonoBehaviour
{
    [SerializeField] private GameObject videoScreen;
    [SerializeField] private VideoPlayer videoPlayer;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnPoked);
        videoScreen.SetActive(false);
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(FadeOut());
    }

    private void OnPoked(SelectEnterEventArgs args)
    {
        videoScreen.SetActive(true);
        videoPlayer.Play();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float duration = 1f;
        float elapsed = 0f;

        Material mat = videoScreen.GetComponent<Renderer>().material;
        Color color = mat.color;
        color.a = 0f;
        mat.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            mat.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float duration = 1f;
        float elpased = 0f;

        Material mat = videoScreen.GetComponent <Renderer>().material;
        Color color = mat.color;

        while (elpased < duration)
        {
            elpased += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elpased / duration));
            mat.color = color;
            yield return null;
        }

        videoScreen.SetActive(false);
    }
}
