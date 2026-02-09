using System;
using UnityEngine;

public abstract class InteractionModuleBase : MonoBehaviour
{
    /// Fired once when this interaction is considered complete for the current activation.
    public event Action Completed;

    public bool IsActive { get; private set; }
    public bool IsComplete { get; private set; }

    public virtual void Activate()
    {
        IsActive = true;
        IsComplete = false;
        // Optional: enabled = true;
    }

    public virtual void Deactivate()
    {
        IsActive = false;
        // Optional: enabled = false;
    }

    protected void Complete()
    {
        if (!IsActive || IsComplete) return;

        IsComplete = true;
        Completed?.Invoke();
    }
}
