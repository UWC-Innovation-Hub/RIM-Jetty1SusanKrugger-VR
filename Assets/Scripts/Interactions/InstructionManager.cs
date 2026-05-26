using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class InstructionManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public Renderer videoRenderer;
    private Material videoMat;

    [Header("Text")]
    public CanvasGroup textCanvas;

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    private void Start()
    {
        videoMat = videoRenderer.material;

        SetMaterialAlpha(videoMat, 1f);
        textCanvas.alpha = 0f;

        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(TransitionToText());
    }

    IEnumerator TransitionToText()
    {
        yield return StartCoroutine(FadeMaterial(videoRenderer, 1, 0));

        yield return StartCoroutine(FadeCanvas(textCanvas, 0, 1));
    }

    public void OnObjectInteracted()
    {
        Debug.Log("Object Interacted!");
        StartCoroutine(FadeOutText());
    }

    IEnumerator FadeOutText()
    {
        yield return StartCoroutine(FadeCanvas(textCanvas, 1f, 0f));
    }

    IEnumerator FadeMaterial(Renderer rend, float start, float end)
    {
        Material mat = rend.material;
        Color color = mat.color;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, time /  fadeDuration);

            color.a = alpha;
            mat.color = color;

            yield return null;
        }

        color.a = end;
        mat.color = color;
    }

    IEnumerator FadeCanvas(CanvasGroup canvas, float start, float end)
    {
        float time = 0f;

        canvas.alpha = start;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(start, end, time / fadeDuration);
            yield return null;
        }

        canvas.alpha = end;
    }

    void SetMaterialAlpha(Material mat, float alpha)
    {
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}
