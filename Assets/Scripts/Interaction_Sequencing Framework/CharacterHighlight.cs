using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterHighlight : MonoBehaviour
{
    [Header("Highlight Visual")]
    [SerializeField] private Renderer[] renderers;

    [SerializeField] private string shaderProperty = "_OutlineAlpha";

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private MaterialPropertyBlock _propBlock;
    private Coroutine _fadeRoutine;
    private float _currentIntensity;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        _propBlock = new MaterialPropertyBlock();
        SetIntensity(0f);
    }

    public void FadeIn()
    {
        StartFade(maxIntensity, fadeInDuration);
    }

    public void FadeOut()
    {
        StartFade(0f, fadeOutDuration);
    }

    public void SetImmediate(bool on)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            SetIntensity(on ? maxIntensity : 0f);
        }
    }

    private void StartFade(float target, float duration)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(target, duration));
        }
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = _currentIntensity;
        float t = 0f;

        if (duration <= 0f)
        {
            SetIntensity(target);
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = fadeCurve.Evaluate(Mathf.Clamp01(t / duration));
            SetIntensity(Mathf.Lerp(start, target, normalized));
            yield return null;
        }

        SetIntensity(target);
        _fadeRoutine = null;
    }

    private void SetIntensity(float value)
    {
        _currentIntensity = value;
        foreach (var rend in renderers)
        {
            if (rend == null)
            {
                continue;
            }
            rend.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(shaderProperty, value);
            rend.SetPropertyBlock(_propBlock);
        }
    }
}
