using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class BlinkingOverlay : MonoBehaviour
{
    public float maxAlpha = 0.45f;
    public float fadeIn = 0.6f;
    public float hold = 0.15f;
    public float fadeOut = 0.9f;
    public float delay = 1.5f;

    private Image img;
    private Color baseColor;

    void Awake()
    {
        img = GetComponent<Image>();
        baseColor = img.color;
        baseColor.a = 0f;
        img.color = baseColor;
    }

    void Start()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return FadeTo(maxAlpha, fadeIn);
            yield return new WaitForSeconds(hold);
            yield return FadeTo(0f, fadeOut);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = img.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float a)
    {
        baseColor.a = a;
        img.color = baseColor;
    }
}
