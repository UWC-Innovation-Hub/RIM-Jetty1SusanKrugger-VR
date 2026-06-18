using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class GazeTarget : MonoBehaviour, IGazeTarget
{
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

    [SerializeField] private Material HighlightMaterial;

    [Header("Events")]
    public UnityEvent onGazeEnter;
    public UnityEvent onGazeExit;
    public UnityEvent onGazeDwell;

    private Coroutine animationCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine indicatorCoroutine;
    private Vector3 indicatorOriginalScale;
    private GazeIndicator indicatorScript;

    private bool playedAudio = false;
    private bool playedVideo = false;

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
    }

    public void SetEmissionMatUp()
    {
        HighlightMaterial.SetFloat("_EmissionStrength", 2.0f);
    }

    public void SetEmissionMatDown()
    {
        HighlightMaterial.SetFloat("_EmissionStrength", 0.0f);
    }

    public void OnGazeEnter()
    {
        onGazeEnter?.Invoke();

        if (playVideoOnGaze && videoPlayer != null)
        {
            if (!playVideoOnlyOnce || !playedVideo)
            {
                videoPlayer.Play();
                playedVideo = true;
                Debug.Log("Video is playing");
            }
        }

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
        onGazeExit?.Invoke();

        StopRoutine(ref hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void OnGazeDwell()
    {
        onGazeDwell?.Invoke();

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
