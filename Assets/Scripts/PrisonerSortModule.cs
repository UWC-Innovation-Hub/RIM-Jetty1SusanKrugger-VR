using UnityEngine;

public class PrisonerSortModule : InteractionModuleBase
{
    [Header("Completion Rule")]
    [SerializeField] private int requiredSorted = 5;

    [Header("Batch Reset Wiring")]
    [SerializeField] private ReactivateRoutes routeController;
    [SerializeField] private PrisonerRoute prisonerRoute;

    public int SortedCount { get; private set; }

    private void Reset()
    {
        ResolveDependencies();
    }

    public override void Activate()
    {
        base.Activate();
        SortedCount = 0;
        ResolveDependencies();

        if (routeController != null)
            routeController.ResetForBatch();

        if (prisonerRoute != null)
            prisonerRoute.ResetForBatch();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        ResolveDependencies();

        if (routeController != null)
            routeController.EndPrisonerSortInteraction();
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

    private void ResolveDependencies()
    {
        if (!routeController) routeController = GetComponent<ReactivateRoutes>();
        if (!prisonerRoute) prisonerRoute = FindFirstObjectByType<PrisonerRoute>();
    }
}
