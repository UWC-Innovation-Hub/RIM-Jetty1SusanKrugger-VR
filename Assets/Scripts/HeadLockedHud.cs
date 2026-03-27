using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HeadLockedHud : MonoBehaviour
{
    [Header("Anchor")]
    [SerializeField] private Transform anchorOverride;
    [SerializeField] private bool attachToCenterEyeOnAwake = true;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.08f, 0.6f);
    [SerializeField] private Vector3 localEulerAngles;

    [Header("UI")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool fadeWholeCanvas;

    [Header("Fade")]
    [Min(0f)]
    [SerializeField] private float defaultFadeDuration = 0.4f;

    private Coroutine _fadeRoutine;
    private Coroutine _messageRoutine;

    private void Reset()
    {
        targetCanvas = GetComponent<Canvas>();
        messageText = GetComponentInChildren<TextMeshProUGUI>(true);
        canvasGroup = GetComponent<CanvasGroup>();
        anchorOverride = FindAnchorTransform();

        if (targetCanvas != null)
        {
            targetCanvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void Awake()
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponent<Canvas>();
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (targetCanvas != null)
        {
            targetCanvas.renderMode = RenderMode.WorldSpace;
        }

        if (attachToCenterEyeOnAwake)
        {
            AttachToAnchor();
        }
    }

    public void AttachToAnchor()
    {
        Transform anchor = anchorOverride != null ? anchorOverride : FindAnchorTransform();
        if (anchor == null)
        {
            Debug.LogWarning($"{name}: No CenterEyeAnchor or Main Camera was found for the HUD.");
            return;
        }

        anchorOverride = anchor;
        transform.SetParent(anchor, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.Euler(localEulerAngles);
    }

    public void SetMessage(string message, bool makeVisible = true)
    {
        if (messageText == null)
        {
            Debug.LogWarning($"{name}: No TMP text is assigned to the head-locked HUD.");
            return;
        }

        messageText.text = message;

        if (makeVisible)
        {
            StopFadeRoutine();
            SetAlpha(1f);
        }
    }

    public void SetMessageFromTimeline(string message)
    {
        SetMessage(message, makeVisible: true);
    }

    public void ClearMessage(bool hide = false)
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        if (hide)
        {
            HideImmediate();
        }
    }

    public void ClearMessageFromTimeline()
    {
        ClearMessage(hide: true);
    }

    public void ShowMessage(string message, float visibleDuration, float fadeDuration = -1f)
    {
        StopMessageRoutine();
        SetMessage(message, makeVisible: true);
        _messageRoutine = StartCoroutine(ShowMessageRoutine(visibleDuration, ResolveDuration(fadeDuration)));
    }

    public void FadeOutText(float duration = -1f)
    {
        StartFade(0f, ResolveDuration(duration));
    }

    public void FadeOutFromTimeline()
    {
        FadeOutText();
    }

    public void FadeInText(float duration = -1f)
    {
        StartFade(1f, ResolveDuration(duration));
    }

    public void FadeInFromTimeline()
    {
        FadeInText();
    }

    public void HideImmediate()
    {
        StopMessageRoutine();
        StopFadeRoutine();
        SetAlpha(0f);
    }

    private IEnumerator ShowMessageRoutine(float visibleDuration, float fadeDuration)
    {
        if (visibleDuration > 0f)
        {
            yield return new WaitForSeconds(visibleDuration);
        }

        yield return FadeRoutine(0f, fadeDuration);
        _messageRoutine = null;
    }

    private void StartFade(float targetAlpha, float duration)
    {
        StopFadeRoutine();
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = GetCurrentAlpha();

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
            _fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, normalized));
            yield return null;
        }

        SetAlpha(targetAlpha);
        _fadeRoutine = null;
    }

    private float GetCurrentAlpha()
    {
        if (fadeWholeCanvas && canvasGroup != null)
        {
            return canvasGroup.alpha;
        }

        return messageText != null ? messageText.alpha : 1f;
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (fadeWholeCanvas && canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            return;
        }

        if (messageText != null)
        {
            messageText.alpha = alpha;
        }
    }

    private void StopFadeRoutine()
    {
        if (_fadeRoutine == null)
        {
            return;
        }

        StopCoroutine(_fadeRoutine);
        _fadeRoutine = null;
    }

    private void StopMessageRoutine()
    {
        if (_messageRoutine == null)
        {
            return;
        }

        StopCoroutine(_messageRoutine);
        _messageRoutine = null;
    }

    private float ResolveDuration(float duration)
    {
        return duration >= 0f ? duration : defaultFadeDuration;
    }

    private static Transform FindAnchorTransform()
    {
        GameObject centerEye = GameObject.Find("CenterEyeAnchor");
        if (centerEye != null)
        {
            return centerEye.transform;
        }

        return Camera.main != null ? Camera.main.transform : null;
    }
}
