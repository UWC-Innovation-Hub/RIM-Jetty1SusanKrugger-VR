using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MaterialOpacityFader : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int opacityMaterialIndex = 0;
    [SerializeField] private string opacityProperty = "_Opacity";
    [SerializeField] private float startOpacity = 5.84f;
    [SerializeField] private float endOpacity = 0f;
    [SerializeField] private int outlineMaterialIndex = 1;
    [SerializeField] private string outlineOpacityProperty = "_OutlineOpacity";
    [SerializeField] private float startOutlineOpacity = 0.4f;
    [SerializeField] private float endOutlineOpacity = 0f;
    [SerializeField] private float fadeDuration = 10f;

    private Material[] _materialInstances;
    private Coroutine _fadeRoutine;

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            enabled = false;
            return;
        }

        _materialInstances = targetRenderer.materials;
        SetOpacity(startOpacity);
        SetOutlineOpacity(startOutlineOpacity);
    }

    public void BeginFade()
    {
        BeginFadeOut();
    }

    public void BeginFadeIn()
    {
        StartFade(hiddenOpacity: endOpacity, visibleOpacity: startOpacity,
            hiddenOutlineOpacity: endOutlineOpacity, visibleOutlineOpacity: startOutlineOpacity);
    }

    public void BeginFadeOut()
    {
        StartFade(hiddenOpacity: startOpacity, visibleOpacity: endOpacity,
            hiddenOutlineOpacity: startOutlineOpacity, visibleOutlineOpacity: endOutlineOpacity);
    }

    public void ResetFade()
    {
        ShowInstant();
    }

    public void ShowInstant()
    {
        StopFadeRoutine();
        SetOpacity(startOpacity);
        SetOutlineOpacity(startOutlineOpacity);
    }

    public void HideInstant()
    {
        StopFadeRoutine();
        SetOpacity(endOpacity);
        SetOutlineOpacity(endOutlineOpacity);
    }

    private IEnumerator FadeRoutine(
        float fromOpacity,
        float toOpacity,
        float fromOutlineOpacity,
        float toOutlineOpacity)
    {
        SetOpacity(fromOpacity);
        SetOutlineOpacity(fromOutlineOpacity);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetOpacity(Mathf.Lerp(fromOpacity, toOpacity, t));
            SetOutlineOpacity(Mathf.Lerp(fromOutlineOpacity, toOutlineOpacity, t));
            yield return null;
        }

        SetOpacity(toOpacity);
        SetOutlineOpacity(toOutlineOpacity);
        _fadeRoutine = null;
    }

    private void SetOpacity(float value)
    {
        Material material = GetMaterial(opacityMaterialIndex);
        if (material != null && material.HasProperty(opacityProperty))
        {
            material.SetFloat(opacityProperty, value);
        }
    }

    private void SetOutlineOpacity(float value)
    {
        Material material = GetMaterial(outlineMaterialIndex);
        if (material != null && material.HasProperty(outlineOpacityProperty))
        {
            material.SetFloat(outlineOpacityProperty, value);
        }
    }

    private Material GetMaterial(int index)
    {
        if (_materialInstances == null || index < 0 || index >= _materialInstances.Length)
        {
            return null;
        }

        return _materialInstances[index];
    }

    private void StartFade(
        float hiddenOpacity,
        float visibleOpacity,
        float hiddenOutlineOpacity,
        float visibleOutlineOpacity)
    {
        if (!enabled || _materialInstances == null || _materialInstances.Length == 0)
        {
            return;
        }

        StopFadeRoutine();
        _fadeRoutine = StartCoroutine(FadeRoutine(
            hiddenOpacity,
            visibleOpacity,
            hiddenOutlineOpacity,
            visibleOutlineOpacity));
    }

    private void StopFadeRoutine()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }
}
