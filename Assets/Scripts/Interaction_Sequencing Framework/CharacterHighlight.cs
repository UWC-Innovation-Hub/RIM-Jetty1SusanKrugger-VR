using System.Collections;
using UnityEngine;


public class CharacterHighlight : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Material glowMat;
    [SerializeField] private Renderer glowRenderer;
    [SerializeField] private int materialIndex = 0;

    [SerializeField] private Collider triggerCollider;

    [Header("Shader Property")]
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float highlightedEmissionStrength = 2f;
    [SerializeField] private float idleEmissionStrength = 0f;

    [Header("Fade Timing")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private Material _instanceMaterial;

    private float _lerpStart;
    private float _lerpTarget;
    private float _lerpDuration;
    private float _lerpElapsed;
    private bool _isLerping;

    private void Awake()
    {
        if (glowMat == null)
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': glowMat is not assigned.");
            return;
        }
        
        if (glowRenderer == null)
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': glowRenderer is not assigned.");
            return;
        }

        _instanceMaterial = new Material(glowMat);

        Material[] mats = glowRenderer.materials;

        if (materialIndex >= 0 && materialIndex < mats.Length)
        {
            mats[materialIndex] = _instanceMaterial;
            glowRenderer.materials = mats;
        }
        else
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': materialIndex {materialIndex} out of range.");
            return;
        }

        if (_instanceMaterial.HasProperty(emissionStrengthProperty))
        {
            _instanceMaterial.SetFloat(emissionStrengthProperty, idleEmissionStrength);
        }
        else
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': material has no property '{emissionStrengthProperty}'.");
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

        _instanceMaterial.SetFloat(emissionStrengthProperty, value);

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

        _lerpStart = _instanceMaterial.GetFloat(emissionStrengthProperty);
        _lerpTarget = target;
        _lerpDuration = duration;
        _lerpElapsed = 0f;
        _isLerping = true;
    }

    private void OnDestroy()
    {
        if (_instanceMaterial != null)
        {
            Destroy(_instanceMaterial);
        }
    }
}
