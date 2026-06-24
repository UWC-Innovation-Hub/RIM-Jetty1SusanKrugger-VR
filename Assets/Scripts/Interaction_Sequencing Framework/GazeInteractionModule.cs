using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GazeInteractionModule : InteractionModuleBase
{
    [System.Serializable]
    private class GazeTargetBinding
    {
        public GazeTarget target;
        public Material[] highlightMaterials;
    }

    //INSPECTOR

    [Header("Wiring")]
    [Tooltip("All objects the user should look at.")]
    [SerializeField] private List<GazeTargetBinding> gazeTargets = new List<GazeTargetBinding>();

    [Header("Complete Rule")]
    [Tooltip("How many objects must be gazed at to complete. -1 means all of them.")]
    [SerializeField] private int requiredGazedCount = -1;

    [Header("Dwell Time")]
    [Tooltip("When enabled, gaze completion is accepted from GazeTarget.OnGazeDwell. When disabled, gaze enter completes immediately.")]
    [SerializeField] private bool useDwellTime = true;

    [Header("Highlight")]
    [Tooltip("Highlight un-gazed objects so the student knows what to look at")]
    [SerializeField] private bool useHighlight = true;
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float highlightedEmissionStrength = 2f;
    [SerializeField] private float idleEmmissionStrength = 0f;

    //PUBLIC STATE

    public int GazedCount => _gazedTargets.Count;

    public int RequiredCount => requiredGazedCount < 0 ? gazeTargets.Count : Mathf.Min(requiredGazedCount, gazeTargets.Count);

    //PRIVATE STATE

    private readonly HashSet<GazeTarget> _gazedTargets = new HashSet<GazeTarget>();
    private readonly Dictionary<GazeTarget, UnityAction> _enterActions = new Dictionary<GazeTarget, UnityAction>();
    private readonly Dictionary<GazeTarget, UnityAction> _dwellActions = new Dictionary<GazeTarget, UnityAction>();
    private bool _subscribed;

    //INTERACTIONMODULEBASE OVERRIDES

    public override void Activate()
    {
        base.Activate();

        _gazedTargets.Clear();

        SubscribeGazeEvents();
        SetAllHighlights(idleEmmissionStrength);
        UpdateHighlights();
        TryCompleteIfReady();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        UnsubscribeGazeEvents();
        SetAllHighlights(idleEmmissionStrength);
    }

    //GAZE EVENT WIRING

    private void SubscribeGazeEvents()
    {
        if (_subscribed)
        {
            return;
        }

        foreach (GazeTargetBinding binding in gazeTargets)
        {
            if (binding?.target == null)
            {
                continue;
            }

            GazeTarget captured = binding.target;

            UnityAction enterAction = () => OnGazeEnter(captured);
            UnityAction dwellAction = () => OnGazeDwell(captured);

            _enterActions[captured] = enterAction;
            _dwellActions[captured] = dwellAction;

            if (useDwellTime)
            {
                binding.target.onGazeDwell.AddListener(dwellAction);
            }
            else
            {
                binding.target.onGazeEnter.AddListener(enterAction);
            }
        }

        _subscribed = true;
    }

    private void UnsubscribeGazeEvents()
    {
        if (!_subscribed)
        {
            return;
        }

        foreach (GazeTargetBinding binding in gazeTargets)
        {
            if (binding?.target == null)
            {
                continue;
            }

            if (_enterActions.TryGetValue(binding.target, out UnityAction enterAction))
            {
                binding.target.onGazeEnter.RemoveListener(enterAction);
            }

            if (_dwellActions.TryGetValue(binding.target, out UnityAction dwellAction))
            {
                binding.target.onGazeDwell.RemoveListener(dwellAction);
            }
        }

        _enterActions.Clear();
        _dwellActions.Clear();
        _subscribed = false;
    }

    //Gaze callbacks

    private void OnGazeEnter(GazeTarget target)
    {
        if (!IsActive || IsComplete || target == null)
        {
            return;
        }

        if (_gazedTargets.Contains(target))
        {
            return;
        }

        AcceptGaze(target);
    }

    private void OnGazeDwell(GazeTarget target)
    {
        if (!IsActive || IsComplete || target == null)
        {
            return;
        }

        if (_gazedTargets.Contains(target))
        {
            return;
        }

        AcceptGaze(target);
    }

    //PROGRESS TRACKING

    private void AcceptGaze(GazeTarget target)
    {
        if (!_gazedTargets.Add(target))
        {
            return;
        }
        Debug.Log($"[GazeInteractionModule] Gazed at: {target.name} ({_gazedTargets.Count}/{RequiredCount})");

        UpdateHighlights();
        TryCompleteIfReady();
    }

    private void TryCompleteIfReady()
    {
        if (_gazedTargets.Count >= RequiredCount)
        {
            Debug.Log("[GazeInteractionsModule] All required objects gazed at - completing.");
            Complete();
        }
    }

    //HIGHLIGHTS

    private void UpdateHighlights()
    {
        if (!useHighlight)
        {
            return;
        }

        foreach (GazeTargetBinding binding in gazeTargets)
        {
            if (binding?.target == null)
            {
                continue;
            }

            bool alreadyGazed = _gazedTargets.Contains(binding.target);
            SetBindingHighlight(binding, alreadyGazed ? idleEmmissionStrength : highlightedEmissionStrength);
        }
    }

    private void SetAllHighlights(float strength)
    {
        if (!useHighlight)
        {
            return;
        }

        foreach (GazeTargetBinding binding in gazeTargets)
        {
            SetBindingHighlight(binding, strength);
        }
    }

    private void SetBindingHighlight(GazeTargetBinding binding, float strength)
    {
        if (binding?.highlightMaterials == null)
        {
            return;
        }

        foreach (Material mat in binding.highlightMaterials)
        {
            if (mat != null && mat.HasProperty(emissionStrengthProperty))
            {
                mat.SetFloat(emissionStrengthProperty, strength);
            }
        }
    }

}
