using System.Collections;
using UnityEngine;


public class CharacterHighlight : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Material glowMat;

    [SerializeField] private Collider triggerCollider;

    [Header("Shader Property")]
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float highlightedEmissionStrength = 2f;
    [SerializeField] private float idleEmissionStrength = 0f;

    [Header("Fade Timing")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private float _lerpStart;
    private float _lerpTarget;
    private float _lerpDuration;
    private float _lerpElapsed;
    private bool _isLerping;

    private void Awake()
    {
        if (glowMat != null && glowMat.HasProperty(emissionStrengthProperty))
        {
           glowMat.SetFloat(emissionStrengthProperty, idleEmissionStrength);
        }
        else if (glowMat == null)
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': glowMat is not assigned.");
        }
        else
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': glowMat has no property '{emissionStrengthProperty}'.");
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (!_isLerping || glowMat == null)
        {
            return;
        }
        _lerpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_lerpElapsed / Mathf.Max(0.0001f, _lerpDuration));
        float value = Mathf.Lerp(_lerpStart, _lerpTarget, t);

        glowMat.SetFloat(emissionStrengthProperty, value);

        if (t >= 1f)
        {
            _isLerping = false;
        }
    }

    public void Show()
    {
        Debug.Log($"[CharacterHighlight] Show() called on '{name}'.");

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
        StartLerp(highlightedEmissionStrength, fadeInDuration);
    }

    public void Hide()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        StartLerp(idleEmissionStrength, fadeOutDuration);
    }

    private void StartLerp(float target, float duration)
    {
        if (glowMat == null || !glowMat.HasProperty(emissionStrengthProperty))
        {
            return;
        }

        _lerpStart = glowMat.GetFloat(emissionStrengthProperty);
        _lerpTarget = target;
        _lerpDuration = duration;
        _lerpElapsed = 0f;
        _isLerping = true;
    }
}
