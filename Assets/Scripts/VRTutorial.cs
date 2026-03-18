using UnityEngine;
using TMPro;

public class VRTutorial : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI tutorialText;

    [Header("Tutorial Steps")]
    [TextArea]
    public string[] steps;

    private int currentStep = 0;

    void Start()
    {
        UpdateText();
    }

    public void NextStep()
    {
        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        tutorialText.text = steps[currentStep];
    }
}
