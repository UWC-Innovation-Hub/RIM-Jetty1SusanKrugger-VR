using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("Progress Settings")]
    public int totalObjects = 3;
    public int totalProgressSteps = 5;

    private int currentCount = 0;
    private int currentStep = 0;

    [Header("UI")]
    public Image progressBar;

    public void AddInteraction()
    {
        currentCount++;

        if (currentCount % totalObjects == 0)
        {
            IncreaseProgress();
        }
    }

    void IncreaseProgress()
    {
        if (currentStep >= totalProgressSteps)
        {
            return;
        }

        currentStep++;

        float progress = (float) currentStep / totalProgressSteps;
        StartCoroutine(SmoothProgress(progress));

        Debug.Log("Progress Step: " + currentStep + " / " + totalProgressSteps);
    }

    IEnumerator SmoothProgress(float target)
    {
        float start = progressBar.fillAmount;
        float t = 0;

        while(t < 1)
        {
            t += Time.deltaTime * 2f;
            progressBar.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }
}
