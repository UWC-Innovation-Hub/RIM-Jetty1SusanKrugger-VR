using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class GazeTarget : MonoBehaviour, IGazeTarget, IGazeProgressTarget
{
    [Header("Interaction")]
    [Tooltip("Prevent progress and dwell from triggering again after the first completed dwell.")]
    [SerializeField] private bool completeOnlyOnce = false;

    [Header("UI")]
    [Tooltip("World-space canvas shown on gaze")]
    [SerializeField] private GameObject infoCanvas;

    [Tooltip("Visual indicator hidden when gazed at.")]
    [SerializeField] private GameObject indicator;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve retractCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Exit Timing")]
    [SerializeField] private float lingerDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Play the assigned AudioSource when gaze dwell completes.")]
    [SerializeField] private bool playAudioOnGaze = true;
    [SerializeField] private bool playOnlyOnce = true;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool playVideoOnGaze = true;
    [SerializeField] private bool playVideoOnlyOnce = true;
    [SerializeField] private float videoFadeDuration = 0.5f;

    [SerializeField] private Material HighlightMaterial;

    [Header("Progress Reticle")]
    [SerializeField] private bool showProgressReticle = false;
    [SerializeField] private Sprite progressSprite;
    [SerializeField] private Vector2 progressReticleSize = new Vector2(100f, 100f);
    [SerializeField] private Vector3 progressReticleLocalPosition = new Vector3(0f, 0f, -0.04f);
    [SerializeField] private Vector3 progressReticleLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 progressReticleLocalScale = new Vector3(0.0004f, 0.0004f, 0.0004f);
    [SerializeField] private Color progressBackgroundColor = new Color(0.8962264f, 0.8962264f, 0.8962264f, 0.2f);
    [SerializeField] private Color progressFillColor = new Color(1f, 0.925f, 0.6f, 1f);

    [Header("Events")]
    public UnityEvent onGazeEnter;
    public UnityEvent onGazeExit;
    public UnityEvent onGazeDwell;

    private Coroutine animationCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine indicatorCoroutine;
    private Coroutine videoPlaybackCoroutine;
    private Vector3 indicatorOriginalScale;
    private GazeIndicator indicatorScript;
    private RectTransform progressReticleRoot;
    private CanvasGroup progressReticleCanvasGroup;
    private Image progressFillArc;
    private Material videoMaterial;
    private int videoColorPropertyId = -1;

    private bool playedAudio = false;
    private bool playedVideo = false;
    private bool hasCompletedDwell = false;
    private bool videoPlaybackFinished = false;
    private bool videoPlaybackFailed = false;

    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    public bool IsVideoPlaybackActive { get; private set; }

    private void Awake()
    {
        if (infoCanvas != null)
        {
            infoCanvas.transform.localScale = Vector3.zero;
            infoCanvas.SetActive(false);
        }

        if (indicator != null)
        {
            indicatorOriginalScale = indicator.transform.localScale;
            indicatorScript = indicator.GetComponent<GazeIndicator>();
        }

        InitializeVideoPlayback();

        EnsureProgressReticle();
        HideProgressReticle();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoPlaybackFinished;
            videoPlayer.errorReceived -= OnVideoPlaybackError;
        }

        if (videoMaterial != null)
        {
            Destroy(videoMaterial);
        }
    }

    public void SetEmissionMatUp()
    {
        HighlightMaterial.SetFloat("_EmissionStrength", 2.0f);
    }
   
    void OnApplicationQuit()
    {
        SetEmissionMatUp();
    }

    public void SetEmissionMatDown()
    {
        HighlightMaterial.SetFloat("_EmissionStrength", 0.0f);
    }

    public void OnGazeEnter()
    {
        onGazeEnter?.Invoke();
        OnGazeProgress(0f);

        StopRoutine(ref hideCoroutine);

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (infoCanvas != null)
        {
            Vector3 currentScale = infoCanvas.activeSelf ? infoCanvas.transform.localScale : Vector3.zero;
            infoCanvas.SetActive(true);
            animationCoroutine = StartCoroutine(AnimateScale(currentScale, Vector3.one, expandCurve));
        }

        if (indicator != null)
        {
            if (indicatorScript != null)
            {
                indicatorScript.enabled = false;
            }

            StopRoutine(ref indicatorCoroutine);
            indicatorCoroutine = StartCoroutine(AnimateIndicator(indicator.transform.localScale, Vector3.zero));
        }
    }

    public void OnGazeExit()
    {
        HideProgressReticle();
        onGazeExit?.Invoke();

        StopRoutine(ref hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void OnGazeDwell()
    {
        if (completeOnlyOnce && hasCompletedDwell)
        {
            HideProgressReticle();
            return;
        }

        if (completeOnlyOnce)
        {
            hasCompletedDwell = true;
        }

        bool videoSelected = playVideoOnGaze && videoPlayer != null;
        bool shouldPlayVideo = videoSelected && (!playVideoOnlyOnce || !playedVideo);

        if (shouldPlayVideo)
        {
            playedVideo = true;
            IsVideoPlaybackActive = true;
        }

        onGazeDwell?.Invoke();
        HideProgressReticle();

        if (videoSelected)
        {
            if (shouldPlayVideo)
            {
                StopRoutine(ref videoPlaybackCoroutine);
                videoPlaybackCoroutine = StartCoroutine(RunVideoPlaybackSequence());
            }

            return;
        }

        if (playAudioOnGaze && audioSource != null)
        {
            if (!playOnlyOnce || !playedAudio)
            {
                audioSource.Play();
                playedAudio = true;
                Debug.Log("Audio is playing");
            }
        }
    }

    public void OnGazeProgress(float normalized)
    {
        if (completeOnlyOnce && hasCompletedDwell)
        {
            HideProgressReticle();
            return;
        }

        if (!showProgressReticle)
        {
            return;
        }

        EnsureProgressReticle();

        if (progressFillArc == null)
        {
            return;
        }

        float fillAmount = Mathf.Clamp01(normalized);
        SetProgressReticleVisible(fillAmount > 0f);
        progressFillArc.color = progressFillColor;
        progressFillArc.fillAmount = fillAmount;
    }

    private void InitializeVideoPlayback()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();
        videoPlayer.loopPointReached += OnVideoPlaybackFinished;
        videoPlayer.errorReceived += OnVideoPlaybackError;

        Renderer targetRenderer = videoPlayer.targetMaterialRenderer;
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name}: VideoPlayer has no target material renderer assigned.");
            return;
        }

        videoMaterial = targetRenderer.material;

        if (videoMaterial.HasProperty(BaseColorPropertyId))
        {
            videoColorPropertyId = BaseColorPropertyId;
        }
        else if (videoMaterial.HasProperty(ColorPropertyId))
        {
            videoColorPropertyId = ColorPropertyId;
        }
        else
        {
            Debug.LogWarning($"{name}: Video material has no supported color property for alpha fading.");
            return;
        }

        SetVideoAlpha(0f);
    }

    private IEnumerator RunVideoPlaybackSequence()
    {
        videoPlaybackFinished = false;
        videoPlaybackFailed = false;
        SetVideoAlpha(0f);

        videoPlayer.Play();
        Debug.Log("Video is playing");

        yield return FadeVideoAlpha(1f);

        if (!videoPlaybackFailed)
        {
            while (!videoPlaybackFinished && !videoPlaybackFailed)
            {
                yield return null;
            }
        }

        yield return FadeVideoAlpha(0f);
        CompleteVideoPlaybackSequence();
    }

    private IEnumerator FadeVideoAlpha(float targetAlpha)
    {
        if (videoMaterial == null || videoColorPropertyId < 0)
        {
            yield break;
        }

        float duration = Mathf.Max(0f, videoFadeDuration);
        float startAlpha = videoMaterial.GetColor(videoColorPropertyId).a;

        if (duration <= 0f)
        {
            SetVideoAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetVideoAlpha(Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetVideoAlpha(targetAlpha);
    }

    private void SetVideoAlpha(float alpha)
    {
        if (videoMaterial == null || videoColorPropertyId < 0)
        {
            return;
        }

        Color color = videoMaterial.GetColor(videoColorPropertyId);
        color.a = Mathf.Clamp01(alpha);
        videoMaterial.SetColor(videoColorPropertyId, color);
    }

    private void OnVideoPlaybackFinished(VideoPlayer source)
    {
        if (source == videoPlayer)
        {
            videoPlaybackFinished = true;
        }
    }

    private void OnVideoPlaybackError(VideoPlayer source, string message)
    {
        if (source != videoPlayer)
        {
            return;
        }

        videoPlaybackFailed = true;
        Debug.LogError($"{name}: Video playback failed: {message}");
    }

    private void CompleteVideoPlaybackSequence()
    {
        SetVideoAlpha(0f);
        IsVideoPlaybackActive = false;
        videoPlaybackCoroutine = null;
    }

    private IEnumerator HideAfterDelay()
    {
        if (lingerDuration > 0f)
        {
            yield return new WaitForSeconds(lingerDuration);
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (infoCanvas != null && infoCanvas.activeSelf)
        {
            Vector3 currentScale = infoCanvas.transform.localScale;
            animationCoroutine = StartCoroutine(AnimateScale(currentScale, Vector3.zero, retractCurve, true, true));
        }

        if (indicator != null)
        {
            StopRoutine(ref indicatorCoroutine);
            indicatorCoroutine = StartCoroutine(AnimateIndicator(indicator.transform.localScale, indicatorOriginalScale));

            if (indicatorScript != null)
            {
                indicatorScript.enabled = true;
            }
        }

        hideCoroutine = null;
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to, AnimationCurve curve, bool disableOnComplete = false, bool invertCurve = false)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float easedT = curve.Evaluate(t);

            if (invertCurve)
            {
                easedT = 1f - easedT;
            }

            easedT = Mathf.Clamp01(easedT);

            infoCanvas.transform.localScale = Vector3.Lerp(from, to, easedT);
            yield return null;
        }

        infoCanvas.transform.localScale = to;

        if (disableOnComplete)
        {
            infoCanvas.SetActive(false);
        }
    }

    private IEnumerator AnimateIndicator(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            indicator.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        indicator.transform.localScale = to;
    }

    private void EnsureProgressReticle()
    {
        if (!showProgressReticle || progressSprite == null)
        {
            return;
        }

        if (progressReticleRoot == null)
        {
            Transform existing = transform.Find("GazeProgressReticle");
            if (existing == null)
            {
                existing = CreateProgressReticleRoot().transform;
            }

            progressReticleRoot = existing as RectTransform;
            progressReticleCanvasGroup = existing.GetComponent<CanvasGroup>();
            progressFillArc = existing.Find("FillArc")?.GetComponent<Image>();
        }

        if (progressReticleRoot == null)
        {
            return;
        }

        if (progressReticleRoot.parent != transform)
        {
            progressReticleRoot.SetParent(transform, false);
        }

        progressReticleRoot.sizeDelta = progressReticleSize;
        progressReticleRoot.localPosition = progressReticleLocalPosition;
        progressReticleRoot.localRotation = Quaternion.Euler(progressReticleLocalEulerAngles);
        progressReticleRoot.localScale = progressReticleLocalScale;

        if (progressFillArc != null)
        {
            progressFillArc.sprite = progressSprite;
            progressFillArc.color = progressFillColor;
            progressFillArc.type = Image.Type.Filled;
            progressFillArc.fillMethod = Image.FillMethod.Radial360;
            progressFillArc.fillOrigin = 0;
            progressFillArc.fillClockwise = true;
            progressFillArc.raycastTarget = false;
        }

        Image backgroundArc = progressReticleRoot.Find("BackgroundArc")?.GetComponent<Image>();
        if (backgroundArc != null)
        {
            backgroundArc.sprite = progressSprite;
            backgroundArc.color = progressBackgroundColor;
            backgroundArc.raycastTarget = false;
        }
    }

    private GameObject CreateProgressReticleRoot()
    {
        GameObject reticleRoot = new GameObject("GazeProgressReticle", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        reticleRoot.layer = gameObject.layer;
        reticleRoot.transform.SetParent(transform, false);

        RectTransform rootRect = reticleRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = progressReticleSize;
        rootRect.localScale = progressReticleLocalScale;
        rootRect.localPosition = progressReticleLocalPosition;
        rootRect.localRotation = Quaternion.Euler(progressReticleLocalEulerAngles);

        Canvas canvas = reticleRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = reticleRoot.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1f;
        scaler.referencePixelsPerUnit = 100f;

        CreateProgressImageChild(reticleRoot.transform, "BackgroundArc", progressBackgroundColor, false);
        CreateProgressImageChild(reticleRoot.transform, "FillArc", progressFillColor, true);

        return reticleRoot;
    }

    private void CreateProgressImageChild(Transform parent, string childName, Color color, bool filled)
    {
        GameObject imageObject = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.sizeDelta = progressReticleSize;
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.sprite = progressSprite;
        image.raycastTarget = false;

        if (!filled)
        {
            return;
        }

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 0;
        image.fillClockwise = true;
        image.fillAmount = 0f;
    }

    private void HideProgressReticle()
    {
        if (progressFillArc != null)
        {
            progressFillArc.fillAmount = 0f;
            progressFillArc.color = progressFillColor;
        }

        SetProgressReticleVisible(false);
    }

    private void SetProgressReticleVisible(bool visible)
    {
        if (progressReticleCanvasGroup == null)
        {
            return;
        }

        progressReticleCanvasGroup.alpha = visible ? 1f : 0f;
        progressReticleCanvasGroup.interactable = false;
        progressReticleCanvasGroup.blocksRaycasts = false;
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null)
        {
            return;
        }

        StopCoroutine(routine);
        routine = null;
    }
}
