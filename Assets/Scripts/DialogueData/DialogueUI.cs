using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private RectTransform buttonContainer;

    [SerializeField] private Button buttonPrefab;

    public event Action<int> OnChoiceSelected;

    private readonly List<Button> _spawnedButtons = new List<Button>();

    private void Awake()
    {
        Hide();
    }

    public void Show(DialogueChoice[] choices)
    {
        ClearButtons();

        for (int i = 0; i < choices.Length; i++)
        {
            Button button = Instantiate(buttonPrefab, buttonContainer);

            SetButtonLabel(button, choices[i].responseText);

            int index = i;
            button.onClick.AddListener(() => OnChoiceSelected?.Invoke(index));

            _spawnedButtons.Add(button);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        ClearButtons();
        gameObject.SetActive(false);
    }

    private void ClearButtons()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            if (_spawnedButtons[i] != null)
            {
                Destroy(_spawnedButtons[i].gameObject);
            }
        }

        _spawnedButtons.Clear();
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
