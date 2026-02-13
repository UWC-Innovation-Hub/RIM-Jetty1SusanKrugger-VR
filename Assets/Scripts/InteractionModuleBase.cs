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
    [Tooltip("Set active while this module is active; set inactive when inactive.")]
    [SerializeField] private GameObject[] activeWhenActive;
    [Tooltip("Force this module's local toggles OFF on scene load until Activate() is called.")]
    [SerializeField] private bool forceInactiveOnAwake = true;

    protected virtual void Awake()
    {
        IsActive = false;
        IsComplete = false;

        if (forceInactiveOnAwake)
        {
            SetEnabled(enableWhenActive, false);
            SetActive(activeWhenActive, false);
        }
    }

    public virtual void Activate()
    {
        IsActive = true;
        IsComplete = false;
        SetEnabled(enableWhenActive, true);
        SetActive(activeWhenActive, true);
    }

    public virtual void Deactivate()
    {
        IsActive = false;
        SetEnabled(enableWhenActive, false);
        SetActive(activeWhenActive, false);
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
