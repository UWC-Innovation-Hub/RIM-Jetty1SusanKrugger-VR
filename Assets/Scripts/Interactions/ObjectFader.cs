using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    private struct ColorEntry
    {
        public Material material;
        public int propertyId;
        public Color original;
    }

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly List<ColorEntry> entries = new List<ColorEntry>();
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private bool intialized;

    private void OnEnable()
    {
        Init();
        SetAlpha(1f);
    }

    private void OnDisable()
    {
        fadeRoutine = null;
    }

    public void FadeOut(float duration)
    {
        Init();

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        SetAlpha(0f);
        fadeRoutine = null;
        gameObject.SetActive(false);
    }

    private void Init()
    {
        if (intialized)
        {
            return;
        }

        intialized = true;

        canvasGroup = GetComponent<CanvasGroup>();

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material m in r.materials)
            {
                int id = m.HasProperty(BaseColorId) ? BaseColorId : m.HasProperty(ColorId) ? ColorId : -1;
                if (id == -1)
                {
                    continue;
                }

                entries.Add(new ColorEntry { material = m, propertyId = id, original = m.GetColor(id) });
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (ColorEntry e in entries)
        {
            Color c = e.original;
            c.a = e.original.a * alpha;
            e.material.SetColor(e.propertyId, c);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
