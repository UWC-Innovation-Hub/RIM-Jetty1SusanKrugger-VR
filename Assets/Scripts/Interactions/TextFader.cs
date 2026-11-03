using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TextFader : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float visibleAlpha = 1f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup _group;
    private Coroutine _routine;

    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 0f;
    }

    public void FadeIn()
    {
        StartFade(visibleAlpha, fadeInDuration);
    }

    public void FadeOut()
    {
        StartFade(0f, fadeOutDuration);
    }

    public void HideInstant()
    {
        if (_group == null)
        {
            _group = GetComponent<CanvasGroup>();
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        _group.alpha = 0f;
    }

    private void StartFade(float target, float duration)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(FadeRoutine(target, duration));
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = _group.alpha;

        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                _group.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }
        }

        _group.alpha = target;
        _routine = null;
    }
}
