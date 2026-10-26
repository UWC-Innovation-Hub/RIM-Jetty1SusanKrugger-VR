using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button buttonYes;

    public event Action<int> OnChoiceSelected;

    private void Awake()
    {
        buttonYes.onClick.AddListener(() => OnChoiceSelected?.Invoke(0));

        Hide();
    }

    public void Show(DialogueChoice[] choices)
    {
       if (choices == null || choices.Length < 2)
        {
            Debug.LogWarning("[DialogueUI] Show() called with fewer than 2 choices.");
        }

       if (choices != null && choices.Length > 0)
        {
            SetButtonLabel(buttonYes, choices[0].responseText);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetButtonLabel(Button button, string label)
    {
        TMPro.TextMeshProUGUI tmp = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = label;
        }
    }
}
