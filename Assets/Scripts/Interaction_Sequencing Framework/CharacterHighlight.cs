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

        if (glowRenderer != null)
        {
            Material[] mats = glowRenderer.materials;
            Debug.Log($"[CharacterHighlight] '{name}': renderer has {mats.Length} material slot(s). Using index {materialIndex}.");
            for (int i = 0; i < mats.Length; i++)
            {
                Debug.Log($"[CharacterHighlight] '{name}': slot {i} = {mats[i]?.name}");
            }

            if (materialIndex >= 0 && materialIndex < mats.Length)
            {
                mats[materialIndex] = _instanceMaterial;
                glowRenderer.materials = mats;
                Debug.Log($"[CharacterHighlight] '{name}': instanced material applied to slot {materialIndex}.");
            }
            else
            {
                Debug.LogWarning($"[CharacterHighlight] '{name}': materialIndex {materialIndex} out of range.");
            }
        }
        else
        {
            Debug.LogWarning($"[CharacterHighlight] '{name}': glowRenderer is not assigned.");
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (!_isLerping || _instanceMaterial == null)
        {
            return;
        }
        _lerpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_lerpElapsed / Mathf.Max(0.0001f, _lerpDuration));
        _instanceMaterial.SetFloat(emissionStrengthProperty, Mathf.Lerp(_lerpStart, _lerpTarget, t));

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
        if (_instanceMaterial == null || !_instanceMaterial.HasProperty(emissionStrengthProperty))
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
