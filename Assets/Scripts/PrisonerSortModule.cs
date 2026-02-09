using UnityEngine;

public class PrisonerSortModule : InteractionModuleBase
{
    [Header("Completion Rule")]
    [SerializeField] private int requiredSorted = 5;
    [SerializeField] private bool autoActivateOnStart = false;

    public int SortedCount { get; private set; }

    private void Start()
    {
        if (autoActivateOnStart)
            Activate();
    }

    public override void Activate()
    {
        base.Activate();
        SortedCount = 0;
    }

    /// Call this when ONE prisoner finishes arriving at their target.
    public void RegisterPrisonerArrived()
    {
        if (!IsActive || IsComplete) return;

        SortedCount++;

        // Debug sanity for now
        Debug.Log($"[PrisonerSortModule] Arrived: {SortedCount}/{requiredSorted}");

        if (SortedCount >= requiredSorted)

            Complete();
    }

    public void DisableInteractables()
    {

    }
}
