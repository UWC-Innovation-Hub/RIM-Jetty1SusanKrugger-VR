using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Oculus.Interaction;
public class VideoTrigger : MonoBehaviour
{
    [SerializeField] private GameObject videoScreen;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Meta Interaction")]
    [SerializeField] private InteractableUnityEventWrapper interactable;

    void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<InteractableUnityEventWrapper>();
        }

        if (interactable != null)
        {
            interactable.WhenSelect.AddListener(OnInteract);
        }
        else
        {
            Debug.LogWarning("[VideoTrigger] No InteractWrapper found");
        }

        videoScreen.SetActive(false);
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.WhenSelect.RemoveListener(OnInteract);
        }

        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(FadeOut());
    }

    private void OnInteract()
    {
        videoScreen.SetActive(true);
        videoPlayer.Play();
        StartCoroutine(FadeIn());
        Debug.Log("Video is playing");
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

        Material mat = videoScreen.GetComponent<Renderer>().material;
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
