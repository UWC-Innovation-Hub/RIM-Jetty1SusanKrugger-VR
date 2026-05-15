using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimedChoiceManager : MonoBehaviour
{
    [Header("Canvas Settings")]
    [Tooltip("Drag your Canvas GameObject here in the Inspector")]
    public Canvas popupCanvas;

    [Header("Panel")]
    public GameObject popupPanel;

    [Header("Timer Settings")]
    public float timerDuration = 10f;
    public float minDelay = 5f;
    public float maxDelay = 10f;

    [Header("UI References")]
    public Image timerImage;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;

    [Header("Pulse Settings")]
    public float pulseThreshold = 3f;
    public float pulseSpeed = 5f;
    public float pulseScale = 1.2f;
    public Color pulseColor = Color.red;

    private Color ogColor;
    private Coroutine pulseCoroutine;
    private CanvasGroup canvasGroup;
    private Coroutine timerCoroutine;

    void Start()
    {
        // Make sure the canvas is hidden at the start
        if (popupCanvas == null)
        {
            Debug.LogWarning("RandomCanvasPopup: No canvas assigned! Please assign a Canvas in the Inspector.");
            return;
        }

        if (popupPanel == null)
        {
            return;
        }

        canvasGroup = popupPanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = popupPanel.AddComponent<CanvasGroup>();
        }

        popupCanvas.gameObject.SetActive(false);
        popupPanel.transform.localScale = Vector3.one;
        canvasGroup.alpha = 0f;

        if (timerImage != null)
        {
            ogColor = timerImage.color;
            timerImage.fillAmount = 1f;
        }

        // Pick a random delay and start the coroutine
        float randomDelay = Random.Range(minDelay, maxDelay);
        Debug.Log($"Canvas will appear in {randomDelay:F2} seconds.");
        StartCoroutine(ShowCanvasAfterDelay(randomDelay));
    }

    private IEnumerator ShowCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("Panel Showing");

        popupPanel.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        if (timerImage != null)
        {
            timerImage.fillAmount = 1f;
            timerImage.color = ogColor;
            timerImage.transform.localScale = Vector3.one;
        }

        popupCanvas.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateOpen());

        timerCoroutine = StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        float timeRemaining = timerDuration;
        bool pulseStarted = false;

        while (timeRemaining > 0)
        {
            if (timerImage != null)
            {
                timerImage.fillAmount = timeRemaining / timerDuration;
            }

            if (!pulseStarted && timeRemaining <= pulseThreshold)
            {
                pulseStarted = true;
                pulseCoroutine = StartCoroutine(PulseTimer());
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        StopPulse();

        if (timerImage != null)
        {
            timerImage.fillAmount = 0f;
        }

        HideCanvas();
    }

    private IEnumerator PulseTimer()
    {
        if (timerImage == null)
        {
            yield break;
        }

        while (true)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float scale = Mathf.Lerp(1f, pulseScale, pulse);
            timerImage.transform.localScale = new Vector3(scale, scale, 1f);

            timerImage.color = Color.Lerp(ogColor, pulseColor, pulse);

            yield return null;
        }
    }

    private void StopPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (timerImage != null)
        {
            timerImage.transform.localScale = Vector3.one;
            timerImage.color = ogColor;
        }
    }

    private IEnumerator AnimateOpen()
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            popupPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, EaseOutBack(t));
            canvasGroup.alpha = Mathf.Clamp01(t * 2f);

            yield return null;
        }

        popupPanel.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        Debug.Log("AnimateOpen finished");
    }

    private IEnumerator AnimateClose()
    {
        Debug.Log("Animate closed");
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            popupPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, EaseInBack(t));
            canvasGroup.alpha = Mathf.Clamp01(1f - t * 2f);

            yield return null;
        }

        popupPanel.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        popupCanvas.gameObject.SetActive(false);
        Debug.Log("Animation closed");
    }

    

    // Optional: call this from a button on your canvas to hide it again
    public void HideCanvas()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        StartCoroutine(AnimateClose());
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.5f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInBack(float t)
    {
        float c1 = 1.5f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}