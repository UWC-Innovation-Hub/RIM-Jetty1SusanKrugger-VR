using System.Collections;
using UnityEngine;

public class EmissionTextureRotator : MonoBehaviour
{
    [SerializeField] private Texture2D[] emissionTextures;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float spinSpeed = 360f;

    private bool isComplete = false;
    
    private Material mat;
    private int currentIndex = -1;
    private Coroutine transitionCoroutine;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        if (mat != null)
        {
            mat.EnableKeyword("_EMISSION");
        }

        mat.SetColor("_EmissionColor", Color.black);
    }

    public void AdvanceStep()
    {
        if (emissionTextures.Length == 0)
        {
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= emissionTextures.Length)
        {
            if (!isComplete)
            {
                isComplete = true;
                StartCoroutine(SpinAnimation());
            }

            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionTo(nextIndex));
    }

    IEnumerator TransitionTo(int nextIndex)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(elapsed / fadeDuration);

            float intensity = Mathf.Lerp(maxIntensity, 0f, t);
            mat.SetColor("_EmissionColor", Color.white * intensity);

            yield return null;
        }

        currentIndex = nextIndex;
        mat.SetTexture("_EmissionMap", emissionTextures[currentIndex]);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(elapsed / fadeDuration);

            float intensity = Mathf.Lerp(0f, maxIntensity, t);
            mat.SetColor("_EmissionColor", Color.white * intensity);

            yield return null;
        }
    }

    IEnumerator SpinAnimation()
    {
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            float spin = spinSpeed * Time.deltaTime;

            transform.Rotate(0f, 0f, spin);

            elapsed += Time.deltaTime;

            yield return null;
        }

    }

    float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}