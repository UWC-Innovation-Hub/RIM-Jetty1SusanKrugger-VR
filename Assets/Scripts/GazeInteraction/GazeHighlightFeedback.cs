using UnityEngine;

[DisallowMultipleComponent]
public class GazeHighlightFeedback : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("GazeTarget to listen to. If blank, this component searches this object and its parents.")]
    [SerializeField] private GazeTarget gazeTarget;

    [Tooltip("Renderers to tint while gazed at. If blank, child renderers are used.")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Base Color Tint")]
    [SerializeField] private bool useBaseColorTint = true;
    [SerializeField] private string baseColorProperty = "_BaseColor";
    [SerializeField] private Color gazeTint = new Color(1f, 0.82f, 0.35f, 1f);
    [Range(0f, 1f)]
    [SerializeField] private float hoverTintAmount = 0.18f;
    [Range(0f, 1f)]
    [SerializeField] private float dwellTintAmount = 0.35f;

    [Header("Emission")]
    [SerializeField] private bool useEmission = true;
    [SerializeField] private string emissionColorProperty = "_EmissionColor";
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private Color emissionColor = new Color(1f, 0.82f, 0.35f, 1f);
    [SerializeField] private float hoverEmissionStrength = 0.45f;
    [SerializeField] private float dwellEmissionStrength = 1.4f;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float dwellPulseDuration = 0.35f;

    private RendererState[] _rendererStates;
    private MaterialPropertyBlock _propertyBlock;
    private float _currentAmount;
    private float _targetAmount;
    private float _pulseRemaining;
    private bool _subscribed;

    private class RendererState
    {
        public Renderer Renderer;
        public Color BaseColor = Color.white;
        public Color EmissionColor = Color.black;
        public bool HasBaseColor;
        public bool HasEmissionColor;
        public bool HasEmissionStrength;
    }

    private void Reset()
    {
        ResolveReferences();
    }

    private void Awake()
    {
        ResolveReferences();
        CacheRendererStates();
        ApplyAmount(0f);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        _currentAmount = 0f;
        _targetAmount = 0f;
        _pulseRemaining = 0f;
        ApplyAmount(0f);
    }

    private void Update()
    {
        float target = _targetAmount;

        if (_pulseRemaining > 0f)
        {
            _pulseRemaining -= Time.deltaTime;
            float pulseT = Mathf.Clamp01(_pulseRemaining / Mathf.Max(0.0001f, dwellPulseDuration));
            target = Mathf.Lerp(_targetAmount, 1f, pulseT);
        }

        float duration = target > _currentAmount ? fadeInDuration : fadeOutDuration;
        float speed = duration <= 0f ? float.PositiveInfinity : 1f / duration;
        _currentAmount = Mathf.MoveTowards(_currentAmount, target, speed * Time.deltaTime);
        ApplyAmount(_currentAmount);
    }

    private void ResolveReferences()
    {
        if (gazeTarget == null)
        {
            gazeTarget = GetComponent<GazeTarget>();
        }

        if (gazeTarget == null)
        {
            gazeTarget = GetComponentInParent<GazeTarget>();
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void CacheRendererStates()
    {
        _propertyBlock = new MaterialPropertyBlock();

        if (targetRenderers == null)
        {
            _rendererStates = new RendererState[0];
            return;
        }

        _rendererStates = new RendererState[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer rendererToCache = targetRenderers[i];
            RendererState state = new RendererState { Renderer = rendererToCache };

            Material material = rendererToCache != null ? rendererToCache.sharedMaterial : null;
            if (material != null)
            {
                state.HasBaseColor = material.HasProperty(baseColorProperty);
                state.HasEmissionColor = material.HasProperty(emissionColorProperty);
                state.HasEmissionStrength = material.HasProperty(emissionStrengthProperty);

                if (state.HasBaseColor)
                {
                    state.BaseColor = material.GetColor(baseColorProperty);
                }

                if (state.HasEmissionColor)
                {
                    state.EmissionColor = material.GetColor(emissionColorProperty);
                }
            }

            _rendererStates[i] = state;
        }
    }

    private void Subscribe()
    {
        if (_subscribed || gazeTarget == null)
        {
            return;
        }

        gazeTarget.onGazeEnter.AddListener(HandleGazeEnter);
        gazeTarget.onGazeDwell.AddListener(HandleGazeDwell);
        gazeTarget.onGazeExit.AddListener(HandleGazeExit);
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || gazeTarget == null)
        {
            return;
        }

        gazeTarget.onGazeEnter.RemoveListener(HandleGazeEnter);
        gazeTarget.onGazeDwell.RemoveListener(HandleGazeDwell);
        gazeTarget.onGazeExit.RemoveListener(HandleGazeExit);
        _subscribed = false;
    }

    private void HandleGazeEnter()
    {
        _targetAmount = hoverTintAmount;
    }

    private void HandleGazeDwell()
    {
        _targetAmount = dwellTintAmount;
        _pulseRemaining = Mathf.Max(0f, dwellPulseDuration);
    }

    private void HandleGazeExit()
    {
        _targetAmount = 0f;
        _pulseRemaining = 0f;
    }

    private void ApplyAmount(float amount)
    {
        if (_rendererStates == null || _propertyBlock == null)
        {
            return;
        }

        for (int i = 0; i < _rendererStates.Length; i++)
        {
            RendererState state = _rendererStates[i];
            if (state?.Renderer == null)
            {
                continue;
            }

            state.Renderer.GetPropertyBlock(_propertyBlock);

            if (useBaseColorTint && state.HasBaseColor)
            {
                Color tinted = Color.Lerp(state.BaseColor, gazeTint, amount);
                _propertyBlock.SetColor(baseColorProperty, tinted);
            }

            if (useEmission && state.HasEmissionColor)
            {
                Color targetEmission = emissionColor * Mathf.Lerp(0f, dwellEmissionStrength, amount);
                _propertyBlock.SetColor(emissionColorProperty, state.EmissionColor + targetEmission);
            }

            if (useEmission && state.HasEmissionStrength)
            {
                float strength = Mathf.Lerp(0f, dwellEmissionStrength, amount);
                if (amount <= hoverTintAmount + 0.0001f)
                {
                    strength = Mathf.Lerp(0f, hoverEmissionStrength, Mathf.InverseLerp(0f, hoverTintAmount, amount));
                }

                _propertyBlock.SetFloat(emissionStrengthProperty, strength);
            }

            state.Renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
