using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 10f;
    public float fadeDuration = 1f;

    [Header("References")]
    public TextMeshProUGUI timerText;
    public Image barFill;
    public Image barBack;
    public BarShake barShake;

    private float timeRemaining;
    private float fadeTimer = 0f;
    private enum State { FadingIn, Running, FadingOut, Done }
    private State state = State.FadingIn;

    void Start()
    {
        timeRemaining = duration;
        SetAlpha(0f);
    }

    void Update()
    {
        if (state == State.FadingIn)
        {
            fadeTimer += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(fadeTimer / fadeDuration));
            if (fadeTimer >= fadeDuration)
            {
                fadeTimer = 0f;
                state = State.Running;
            }
        }
        else if (state == State.Running)
        {
            if (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                UpdateDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0f;
                UpdateDisplay(0f);
                state = State.FadingOut;
                timerText.text = "Done!";
            }
        }
        else if (state == State.FadingOut)
        {
            fadeTimer += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(1f - (fadeTimer / fadeDuration)));
            if (fadeTimer >= fadeDuration)
            {
                SetAlpha(0f);
                state = State.Done;
            }
        }
    }

    void UpdateDisplay(float time)
    {
        float fraction = time / duration;
        barFill.fillAmount = fraction;
        barShake.UpdateShake(fraction);
    }

    void SetAlpha(float alpha)
    {
        timerText.alpha = alpha;
        SetImageAlpha(barFill, alpha);
        SetImageAlpha(barBack, alpha);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
