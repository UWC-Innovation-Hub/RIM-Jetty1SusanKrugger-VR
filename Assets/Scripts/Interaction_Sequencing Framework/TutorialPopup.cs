using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button closeButton;

    public event Action Closed;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnCloseClicked()
    {
        Hide();
        Closed?.Invoke();
    }
}
