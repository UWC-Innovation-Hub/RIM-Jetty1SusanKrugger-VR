using UnityEngine;
using TMPro;

public class TenSecTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 10f;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning;

    void OnEnable()
    {
        currentTime = startTime;
        isRunning = true;
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
        timerText.text = Mathf.Ceil(currentTime).ToString();
    }

    void OnTimerEnd()
    {
        Debug.Log("Timer Done");
    }
}
