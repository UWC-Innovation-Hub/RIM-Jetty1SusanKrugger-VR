using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Manages the opening blackout fade for C1_J2_Warden_Experience.
/// Attach to the BlackOverlay UI Image (which also has a CanvasGroup component).
///
/// Setup:
///   1. Add a CanvasGroup to the BlackOverlay GameObject.
///   2. Assign it to the canvasGroup field.
///   3. Call BeginFade() from Timeline Signal Emitter at time 0,
///      OR enable autoStartOnAwake to trigger on scene load automatically.
/// </summary>
public class BlackoutFade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [Tooltip("Seconds of pure black before the fade begins (the silence beat)")]
    [SerializeField] private float silenceDelay = 3f;

    [Tooltip("How long the fade from black to fully visible takes")]
    [SerializeField] private float fadeDuration = 5f;

    [Header("Behaviour")]
    [Tooltip("If true, BeginFade() is called automatically when the scene loads")]
    [SerializeField] private bool autoStartOnAwake = false;

    [Header("Events")]
    public UnityEvent OnFadeComplete;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private void Start()
    {
        if (autoStartOnAwake)
            BeginFade();
    }

    /// <summary>
    /// Start the blackout sequence.
    /// Wire this to a Signal Emitter at time 0 on C1_Timeline_Inventory,
    /// or toggle autoStartOnAwake for simpler setups.
    /// </summary>
    public void BeginFade()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // Ensure we start fully black
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        // Beat 1: Pure silence, pure black
        yield return new WaitForSeconds(silenceDelay);

        // Beat 2: Fade from black to clear
        if (canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }

        _fadeCoroutine = null;
        OnFadeComplete?.Invoke();
    }

    /// <summary>
    /// Instantly returns to full black — use for scene resets or if a
    /// ForceBlack() call is needed before replaying the opening.
    /// </summary>
    public void ForceBlack()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }
    }
}
