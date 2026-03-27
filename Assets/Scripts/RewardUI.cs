using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Video;
using System.Collections;

public class EndScreen : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Play";
    [SerializeField] private float animationDuration = 2f;

    [Header("Fade Settings")]
    [SerializeField] private Renderer uiRenderer;
    [SerializeField] private float uiFadeDuration = 1f;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Renderer videoRenderer;
    [SerializeField] private float videoFadeDuration = 1f;

    private XRSimpleInteractable xrInteractable;
    private bool hasBeenTriggered = false;

    private void Awake()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
        {
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void Start()
    {
        xrInteractable.selectEntered.AddListener(OnPoke);

        SetAlpha(videoRenderer, 0f);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnPoke(SelectEnterEventArgs args)
    {
        if (hasBeenTriggered)
        {
            return;
        }

        hasBeenTriggered = true;

        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(animationDuration);

        if (animator != null)
        {
            animator.enabled = false;
        }

        yield return StartCoroutine(Fade(uiRenderer, 1f, 0f, uiFadeDuration));

        if (uiRenderer != null)
        {
            uiRenderer.enabled = false;
        }
        
        if (xrInteractable != null)
        {
            xrInteractable.enabled = false;
        }

        yield return StartCoroutine(Fade(videoRenderer, 0f, 1f, videoFadeDuration));

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(VideoFadeOut());
    }

    private IEnumerator VideoFadeOut()
    {
        yield return StartCoroutine(Fade(videoRenderer, 1f, 0f, videoFadeDuration));
    }

    private IEnumerator Fade(Renderer rend, float start, float end, float duration)
    {
        if (rend == null)
        {
            yield break;
        }

        Material mat = rend.material;
        Color color = mat.color;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, timer / duration);

            color.a = alpha;
            mat.color = color;

            yield return null;
        }

        color.a = end;
        mat.color = color;
    }

    private void SetAlpha(Renderer rend, float alpha)
    {
        if (rend == null)
        {
            return;
        }

        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;
    }

    private void OnDestroy()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnPoke);
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}