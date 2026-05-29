using System.Collections;
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
    [Tooltip("Wheather the student must hold their gaze for a set duration to register a look")]
    [SerializeField] private bool useDwellTime = true;
    [SerializeField] private float dwellDuration = 1.5f;

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
    private readonly Dictionary<GazeTarget, Coroutine> _dwellCoroutines = new Dictionary<GazeTarget, Coroutine>();
    private readonly Dictionary<GazeTarget, UnityAction> _enterActions = new Dictionary<GazeTarget, UnityAction>();
    private readonly Dictionary<GazeTarget, UnityAction> _exitActions = new Dictionary<GazeTarget, UnityAction>();
    private bool _subscribed;

    //INTERACTIONMODULEBASE OVERRIDES

    public override void Activate()
    {
        base.Activate();

        _gazedTargets.Clear();
        StopAllDwellCoroutines();

        SubscribeGazeEvents();
        SetAllHighlights(idleEmmissionStrength);
        UpdateHighlights();
        TryCompleteIfReady();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        UnsubscribeGazeEvents();
        StopAllDwellCoroutines();
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
            UnityAction exitAction = () => OnGazeExit(captured);

            _enterActions[captured] = enterAction;
            _exitActions[captured] = exitAction;

            binding.target.onGazeEnter.AddListener(enterAction);
            binding.target.onGazeExit.AddListener(exitAction);
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

            if (_exitActions.TryGetValue(binding.target, out UnityAction exitAction))
            {
                binding.target.onGazeExit.RemoveListener(exitAction);
            }
        }

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

        if (useDwellTime)
        {
            if (!_dwellCoroutines.ContainsKey(target))
            {
                _dwellCoroutines[target] = StartCoroutine(DwellRoutine(target));
            }
        }
        else
        {
            AcceptGaze(target);
        }
    }

    private void OnGazeExit(GazeTarget target)
    {
        if (!IsActive || IsComplete || target == null)
        {
            return;
        }

        if (_dwellCoroutines.TryGetValue(target, out Coroutine routine))
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                _dwellCoroutines.Remove(target);
            }
        }
    }

    //DWELL TIMER

    private IEnumerator DwellRoutine(GazeTarget target)
    {
        yield return new WaitForSeconds(dwellDuration);
        _dwellCoroutines.Remove(target);
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

    //HELPERS

    private void StopAllDwellCoroutines()
    {
        foreach (Coroutine routine in _dwellCoroutines.Values)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }
        _dwellCoroutines.Clear();
    }
}
