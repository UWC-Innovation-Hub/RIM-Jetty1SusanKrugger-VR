using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
public class SaturationFader : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Volume volume;

    [Header("Fade")]
    [SerializeField] private float colorWeight = 0f;
    [SerializeField] private float grayscaleWeight = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine _routine;

    public float FadeDuration => fadeDuration;

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        if (volume == null)
        {
            return;
        }

        volume.weight = colorWeight;
    }

    public void FadeToGrayscale()
    {
        StartFade(grayscaleWeight);
    }

    public void FadeToColor()
    {
        StartFade(colorWeight);
    }

    public void SetColorInstant()
    {
        if (volume == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        volume.weight = colorWeight;
    }

    private void StartFade(float target)
    {
        if (volume == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = volume.weight;

        if (fadeDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / fadeDuration));

                volume.weight = Mathf.Lerp(start, target, t);
                yield return null;
            }
        }

        volume.weight = target;
        _routine = null;
    }
}
