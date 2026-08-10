using System.Collections;
using UnityEngine;

public class CharacterSpotlight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light spotlight;
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light[] pointLights;

    [Header("Spotlight Settings")]
    [SerializeField] private float spotTargetIntensity = 1f;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    private float[] _originalPoint;
    private float _originalDirectional;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (spotlight != null)
        {
            spotlight.intensity = 0f;
            spotlight.enabled = false;
        }

        if (directionalLight != null)
        {
            _originalDirectional = directionalLight.intensity;
        }

        if (pointLights != null)
        {
            _originalPoint = new float[pointLights.Length];
            for (int i = 0; i < pointLights.Length; i++)
            {
                if (pointLights[i] != null)
                {
                    _originalPoint[i] = pointLights[i].intensity;
                }
            }
        }
    }

    // PUBLIC API

    public void Activate()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(spotStart: spotlight != null ? spotlight.intensity : 0f, spotEnd: spotTargetIntensity, dirStart: directionalLight != null ? directionalLight.intensity : 0f, dirEnd: 0f, pointLightsEnd: 0f));
    }

    public void Deactivate()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(spotStart: spotlight != null ? spotlight.intensity : 0f, spotEnd: spotTargetIntensity, dirStart: directionalLight != null ? directionalLight.intensity : 0f, dirEnd: _originalDirectional, pointLightsEnd: -1f, disableSpotOnComplete: true));

    }

    // INTERNAL

    private IEnumerator FadeRoutine(float spotStart, float spotEnd, float dirStart, float dirEnd, float pointLightsEnd, bool disableSpotOnComplete = false)
    {
        if (spotlight != null)
        {
            spotlight.enabled = true;
        }

        float[] pointLightStarts = null;
        if (pointLights != null)
        {
            pointLightStarts = new float[pointLights.Length];
            for (int i = 0; i < pointLights.Length; i++)
            {
                if (pointLights[i] != null)
                {
                    pointLightStarts[i] = pointLights[i].intensity;
                }
            }
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

            if (pointLights != null && pointLightStarts != null)
            {
                for (int i = 0; i < pointLightStarts.Length; i++)
                {
                    if (pointLights[i] == null)
                    {
                        continue;
                    }

                    float target = pointLightsEnd < 0f ? _originalPoint[i] : pointLightsEnd;

                    pointLights[i].intensity = Mathf.Lerp(pointLightStarts[i], target, t);
                }
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

        if (pointLights != null)
        {
            for (int i = 0; i < pointLights.Length; i++)
            {
                if (pointLights[i] == null)
                {
                    continue;
                }

                pointLights[i].intensity = pointLightsEnd < 0f ? _originalPoint[i] : pointLightsEnd;
            }
        }

        _fadeRoutine = null;
    }
}
