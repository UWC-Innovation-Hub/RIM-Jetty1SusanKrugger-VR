using System.Collections;
using UnityEngine;

public class CharacterSpotlight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light spotlight;
    [SerializeField] private Light directionalLight;

    [Header("Spotlight Settings")]
    [SerializeField] private float spotTargetIntensity = 1f;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    private float _originalSpot;
    private float _originalDirectional;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (spotlight != null)
        {
            _originalSpot = spotlight.intensity;
            spotlight.intensity = 0f;
            spotlight.enabled = false;
        }

        if (directionalLight != null)
        {
            _originalDirectional = directionalLight.intensity;
        }
    }

    // PUBLIC API

    public void Activate()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(spotStart: spotlight != null ? spotlight.intensity : 0f, spotEnd: spotTargetIntensity, dirStart: directionalLight != null ? directionalLight.intensity : 0f, dirEnd: 0f));
    }

    public void Deactivate()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(spotStart: spotlight != null ? spotlight.intensity : 0f, spotEnd: spotTargetIntensity, dirStart: directionalLight != null ? directionalLight.intensity : 0f, dirEnd: _originalDirectional, disableSpotOnComplete: true));

    }

    // INTERNAL

    private IEnumerator FadeRoutine(float spotStart, float spotEnd, float dirStart, float dirEnd, bool disableSpotOnComplete = false)
    {
        if (spotlight != null)
        {
            spotlight.enabled = true;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (spotlight != null)
            {
                spotlight.intensity = Mathf.Lerp(spotStart, spotEnd, t);
            }

            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(dirStart, dirEnd, t);
            }

            yield return null;
        }

        if (spotlight != null)
        {
            spotlight.intensity = spotEnd;

            if (disableSpotOnComplete)
            {
                spotlight.enabled = false;
            }
        }

        if (directionalLight != null)
        {
            directionalLight.intensity = dirEnd;
        }

        _fadeRoutine = null;
    }
}
