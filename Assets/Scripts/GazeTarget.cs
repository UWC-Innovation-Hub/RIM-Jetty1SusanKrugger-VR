using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Events")]
    public UnityEvent onGazeEnter;
    public UnityEvent onGazeExit;
    public UnityEvent onGazeDwell;

    private Coroutine animationCoroutine;
    private Vector3 indicatorOriginalScale;
    private GazeIndicator indicatorScript;

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

    public void OnGazeEnter()
    {
        onGazeEnter?.Invoke();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (infoCanvas != null)
        {
            infoCanvas.SetActive(true);
            animationCoroutine = StartCoroutine(AnimateScale(Vector3.zero, Vector3.one, expandCurve));
        }

        if (indicator != null)
        {
            if (indicatorScript != null)
            {
                indicatorScript.enabled = false;
            }

            StartCoroutine(AnimateIndicator(indicator.transform.localScale, Vector3.zero));
        }
    }

    public void OnGazeExit()
    {
        onGazeExit?.Invoke();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (infoCanvas != null)
        {
            Vector3 currentScale = infoCanvas.transform.localScale;
            animationCoroutine = StartCoroutine(AnimateScale(currentScale, Vector3.zero, retractCurve, true));
        }

        if (indicator != null)
        {
            StartCoroutine(AnimateIndicator(Vector3.zero, indicatorOriginalScale));

            if (indicatorScript != null)
            {
                indicatorScript.enabled = true;
            }
        }
    }

    public void OnGazeDwell()
    {
        onGazeDwell?.Invoke();
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to, AnimationCurve curve, bool disableOnComplete = false)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float easedT = curve.Evaluate(t);

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
}
