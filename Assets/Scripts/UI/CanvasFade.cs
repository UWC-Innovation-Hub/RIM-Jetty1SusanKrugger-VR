using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;

public class CanvasFade : MonoBehaviour
{
    [Header("Canvas To Fade")]
    public CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnPressed);
        }
    }

    private void OnPressed()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCanvas());
        }
    }

    private IEnumerator FadeCanvas()
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
