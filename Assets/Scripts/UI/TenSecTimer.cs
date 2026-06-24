using UnityEngine;
using TMPro;
using System.Collections;

public class TenSecTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 10f;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    [Header("Pulse Settings")]
    public float normalScale = 1f;
    public float pulseScale = 1.4f;
    public float pulseDuration = 0.2f;
    public int pulseThreshold = 5;

    [Header("Glow Settings")]
    public Color normalColor = Color.white;
    public Color glowColor = new Color(1f, 0.4f, 0f, 1f);

    private float currentTime;
    private bool isRunning;
    private int lastDisplayedSecond = -1;
    void OnEnable()
    {
        StartTimer();
    }

    void OnDisable()
    {
        StopCoroutine("Pulse");
        timerText.transform.localScale = new Vector3(normalScale, normalScale, 1f);
        timerText.color = normalColor;
    }

    public void StartTimer()
    {
        StopCoroutine("PulseEffect");
        currentTime = startTime;
        isRunning = true;
        lastDisplayedSecond = -1;
        timerText.color = normalColor;
        timerText.transform.localScale = new Vector3(normalScale, normalScale, 1f);
        UpdateTimer();
    }

    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimer();
        }
        else
        {
            currentTime = 0;
            isRunning = false;
            UpdateTimer();
            OnTimerEnd();
        }
    }

    void UpdateTimer()
    {
        int displayedSecond = Mathf.CeilToInt(currentTime);
        timerText.text = Mathf.Ceil(currentTime).ToString();

        if (displayedSecond != lastDisplayedSecond && displayedSecond <= pulseThreshold && displayedSecond > 0)
        {
            lastDisplayedSecond = displayedSecond;
            StopCoroutine("PulseEffect");
            timerText.transform.localScale = new Vector3(normalScale, normalScale, 1f);
            timerText.color = normalColor;
            StartCoroutine("PulseEffect");
        }
    }

    IEnumerator PulseEffect()
    {
        Transform t = timerText.transform;
        float halfDuration = pulseDuration / 2f;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration;
            float scale = Mathf.Lerp(normalScale, pulseScale, elapsed / halfDuration);
            t.localScale = new Vector3(scale, scale, 1f);
            timerText.color = Color.Lerp(normalColor, glowColor, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration;
            float scale = Mathf.Lerp(pulseScale, normalScale, elapsed / halfDuration);
            t.localScale = new Vector3(scale, scale, 1f);
            timerText.color = Color.Lerp(glowColor, normalColor, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.localScale = new Vector3(normalScale, normalScale, 1f);
        timerText.color = normalColor;
    }

    void OnTimerEnd()
    {
        Debug.Log("Timer Done");
    }
}
