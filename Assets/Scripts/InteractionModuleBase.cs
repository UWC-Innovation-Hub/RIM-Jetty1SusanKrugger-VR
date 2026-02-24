using System;
using UnityEngine;

public abstract class InteractionModuleBase : MonoBehaviour
{
    /// Fired once when this interaction is considered complete for the current activation.
    public event Action Completed;

    public bool IsActive { get; private set; }
    public bool IsComplete { get; private set; }

    [Header("Module-local activation toggles")]
    [Tooltip("Enabled while this module is active; disabled when inactive.")]
    [SerializeField] private Behaviour[] enableWhenActive;
    [Tooltip("Disabled while this module is active; enabled when inactive.")]
    [SerializeField] private Behaviour[] disableWhenActive;
    [Tooltip("Set active while this module is active; set inactive when inactive.")]
    [SerializeField] private GameObject[] activeWhenActive;
    [Tooltip("Set inactive while this module is active; set active when inactive.")]
    [SerializeField] private GameObject[] inactiveWhenActive;
    [Tooltip("Force this module's local toggles OFF on scene load until Activate() is called.")]
    [SerializeField] private bool forceInactiveOnAwake = true;

    [Header("Optional Environment")]
    [Tooltip("When active, force RenderSettings.fog to this value.")]
    [SerializeField] private bool enableFog = false;
    [Tooltip("Restore previous RenderSettings.fog value when this module deactivates.")]
    [SerializeField] private bool restoreFogOnDeactivate = true;

    [Header("Optional Transition Fade")]
    [SerializeField] private bool useTransitionFade = false;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;

    private bool _previousFogState;
    private bool _hasFogStateSnapshot;

    public bool UseTransitionFade => useTransitionFade;
    public float FadeOutDuration => fadeOutDuration;
    public float FadeInDuration => fadeInDuration;

    protected virtual void Awake()
    {
        IsActive = false;
        IsComplete = false;

        if (forceInactiveOnAwake)
        {
            SetEnabled(enableWhenActive, false);
            SetEnabled(disableWhenActive, true);
            SetActive(activeWhenActive, false);
            SetActive(inactiveWhenActive, true);
        }
    }

    public virtual void Activate()
    {
        IsActive = true;
        IsComplete = false;
        _previousFogState = RenderSettings.fog;
        _hasFogStateSnapshot = true;

        SetEnabled(enableWhenActive, true);
        SetEnabled(disableWhenActive, false);
        SetActive(activeWhenActive, true);
        SetActive(inactiveWhenActive, false);
        RenderSettings.fog = enableFog;
    }

    public virtual void Deactivate()
    {
        IsActive = false;
        SetEnabled(enableWhenActive, false);
        SetEnabled(disableWhenActive, true);
        SetActive(activeWhenActive, false);
        SetActive(inactiveWhenActive, true);

        if (restoreFogOnDeactivate && _hasFogStateSnapshot)
        {
            RenderSettings.fog = _previousFogState;
        }
    }

    protected void Complete()
    {
        if (!IsActive || IsComplete) return;

        IsComplete = true;
        Completed?.Invoke();
    }

    private static void SetEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null) return;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null) continue;
            behaviours[i].enabled = enabled;
        }
    }

    private static void SetActive(GameObject[] gameObjects, bool active)
    {
        if (gameObjects == null) return;

        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            gameObjects[i].SetActive(active);
        }
    }
}
