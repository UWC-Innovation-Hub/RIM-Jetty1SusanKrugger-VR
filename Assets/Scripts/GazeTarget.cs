using System.Collections;
using UnityEngine;

public class GazeTarget : MonoBehaviour
{
    [Header("UI")]
    public GameObject infoCanvas;
    public GameObject indicator;

    [Header("Animation")]
    public float animationDuration = 0.25f;
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve retractCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Coroutine animationCoroutine;

    private Vector3 indicatorOriginalScale;

    private GazeIndicator indicatorScript;

    void Start()
    {
        infoCanvas.transform.localScale = Vector3.zero;
        infoCanvas.SetActive(false);

        if (indicator != null)
        {
            indicatorOriginalScale = indicator.transform.localScale;
            indicatorScript = indicator.GetComponent<GazeIndicator>();
        }
    }

    public void OnGazeEnter()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        infoCanvas.SetActive(true);
        animationCoroutine = StartCoroutine(AnimateScaleLinear(Vector3.zero, Vector3.one));

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
        if (animationCoroutine == null)
        {
            StopCoroutine(animationCoroutine);
        }

        Vector3 currentScale = infoCanvas.transform.localScale;
        animationCoroutine = StartCoroutine(AnimateScaleLinear(currentScale, Vector3.zero, disableOnComplete: true));

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
        Debug.Log("Interacting with object");
    }

   IEnumerator AnimateScaleLinear(Vector3 from, Vector3 to, bool disableOnComplete = false)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed /  animationDuration;
            infoCanvas.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        infoCanvas.transform.localScale = to;
        if (disableOnComplete) infoCanvas.SetActive(false);
    }

    IEnumerator AnimateIndicator(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        float indicatorAnimDuration = 0.2f;

        while (elapsed < indicatorAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / indicatorAnimDuration;
            indicator.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        indicator.transform.localScale = to;
    }
}
