using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup rootCanvasGroup;

    [Header("Sequenced Elements")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private CanvasGroup imageA;
    [SerializeField] private CanvasGroup imageB;
    [SerializeField] private CanvasGroup imageC;
    [SerializeField] private CanvasGroup buttonCanvasGroup;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float delayBetweenElements = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    public event Action Closed;

    private Coroutine _sequenceRoutine;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClicked);


        SetAlpha(rootCanvasGroup, 0f);
        SetAlpha(panelCanvasGroup, 0f);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        SetAlpha(imageC, 0f);
        SetAlpha(buttonCanvasGroup, 0f);

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        SetAlpha(rootCanvasGroup, 1f);

        if(_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
        }

        _sequenceRoutine = StartCoroutine(FadeInSequence());
    }

    public void Hide()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        _sequenceRoutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInSequence()
    {
        SetAlpha(panelCanvasGroup, 0f);
        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        SetAlpha(imageC, 0f);
        SetAlpha(buttonCanvasGroup, 0f);

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.interactable = false;
        }

        yield return FadeElement(panelCanvasGroup, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(delayBetweenElements);

        yield return FadeElement(imageA, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(delayBetweenElements);

        yield return FadeElement(imageB, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(delayBetweenElements);

        yield return FadeElement(imageC, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(delayBetweenElements);

        yield return FadeElement(buttonCanvasGroup, 0f, 1f, fadeDuration);

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.interactable = true;
        }

        _sequenceRoutine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.interactable = false;
        }

        yield return FadeElement(rootCanvasGroup, 1f, 0f, fadeDuration);

        gameObject.SetActive(false);
        _sequenceRoutine = null;
    }

    private IEnumerator FadeElement(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
        }
    }

    private void OnCloseClicked()
    {
        Hide();
        Closed?.Invoke();
    }
}
