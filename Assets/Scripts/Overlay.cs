using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Overlay : MonoBehaviour
{
    public float maxAlpha = 0.5f;
    public float fadeIn = 0.5f;
    public float hold = 0.2f;
    public float fadeOut = 0.8f;
    public float delay = 1.5f;

    private Material mat;
    private Color baseColor;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        baseColor = mat.color;
        baseColor.a = 0f;
        mat.color = baseColor;
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
        float startAlpha = mat.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer /  duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float a)
    {
        baseColor.a = a;
        mat.color = baseColor;
    }
   
}
