using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Renderer fadeRenderer;
    [SerializeField] private float defaultFadeDuration = 0.5f;

    public bool ShouldFadeOnStart;

    private Material _mat;
    private int _fadeRequestId;

    private void Awake()
    {
        if (!fadeRenderer)
            fadeRenderer = GetComponent<Renderer>();

        if (!fadeRenderer)
        {
            Debug.LogWarning($"{name}: FadeController requires a Renderer.");
            enabled = false;
            return;
        }

        _mat = fadeRenderer.material;
        SetAlpha(1f);
    }

    private void Start()
    {
        if(ShouldFadeOnStart)
        {
            FadeIn();
        }
    }

    public void FadeIn(float duration = -1f)   // Fade from black to clear
    {
        StartCoroutine(FadeInRoutine(duration));
    }

    public void FadeOut(float duration = -1f)  // Fade to black
    {
        StartCoroutine(FadeOutRoutine(duration));
    }

    public IEnumerator FadeInRoutine(float duration = -1f)
    {
        yield return FadeRoutine(0f, duration);
    }

    public IEnumerator FadeOutRoutine(float duration = -1f)
    {
        yield return FadeRoutine(1f, duration);
    }

    public void SetBlackInstant()
    {
        _fadeRequestId++;
        SetAlpha(1f);
    }

    public void SetClearInstant()
    {
        _fadeRequestId++;
        SetAlpha(0f);
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        if (duration < 0f)
            duration = defaultFadeDuration;

        // duration == 0 => instant
        if (duration <= 0f)
        {
            _fadeRequestId++;
            SetAlpha(target);
            yield break;
        }

        int requestId = ++_fadeRequestId;
        float start = _mat.color.a;
        float time = 0f;

        while (time < duration)
        {
            if (requestId != _fadeRequestId)
                yield break;

            time += Time.deltaTime;
            float alpha = Mathf.Lerp(start, target, time / duration);
            SetAlpha(alpha);
            yield return null;
        }

        if (requestId == _fadeRequestId)
            SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        Color c = _mat.color;
        c.a = alpha;
        _mat.color = c;
    }
}
