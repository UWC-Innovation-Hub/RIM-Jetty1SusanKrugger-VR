using System.Collections.Generic;
using UnityEngine;

public class PrisonerSortModule : InteractionModuleBase
{
    [Header("Completion Rule")]
    [SerializeField] private int requiredSorted = 5;

    [Header("Session Authoring")]
    [SerializeField] private PrisonerSortSession[] sessions;
    [SerializeField] private int activeSessionIndex = -1;

    [Header("Batch Reset Wiring")]
    [SerializeField] private ReactivateRoutes routeController;
    [SerializeField] private PrisonerRoute prisonerRoute;

    public int SortedCount { get; private set; }
    public int CurrentBatchIndex { get; private set; }
    public PrisonerSortSession ActiveSession { get; private set; }
    public PrisonerSortBatch CurrentBatch { get; private set; }
    public bool UsesSessionBatches => ActiveSession != null && ActiveSession.GetBatchCount() > 0;

    private readonly HashSet<string> _currentBatchArrivals = new();
    private readonly HashSet<string> _currentBatchFinished = new();

    private void Reset()
    {
        ResolveDependencies();
    }

    public override void Activate()
    {
        base.Activate();
        SortedCount = 0;
        CurrentBatchIndex = 0;
        _currentBatchArrivals.Clear();
        _currentBatchFinished.Clear();
        ResolveDependencies();
        ResolveActiveSession();
        SyncSessionVisibility();

        if (routeController != null)
            routeController.BindSession(ActiveSession);

        if (prisonerRoute != null)
            prisonerRoute.BindSession(ActiveSession);

        ApplyCurrentBatchBinding();

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

        CurrentBatch = null;
        CurrentBatchIndex = 0;
        _currentBatchArrivals.Clear();
        _currentBatchFinished.Clear();

        if (routeController != null)
            routeController.SetActiveBatch(null);

        if (prisonerRoute != null)
            prisonerRoute.SetActiveBatch(null);
    }

    /// Legacy entry point. In session mode this should be routed through RegisterParticipantArrived.
    public void RegisterPrisonerArrived()
    {
        RegisterParticipantArrived(string.Empty);
    }

    public void RegisterParticipantArrived(string participantId)
    {
        if (!IsActive || IsComplete) return;

        if (!UsesSessionBatches)
        {
            RegisterLegacyArrival();
            return;
        }

        if (CurrentBatch == null)
        {
            Debug.LogWarning($"{name}: Received arrival for session mode, but no current batch is bound.");
            return;
        }

        string resolvedId = ResolveParticipantId(participantId, _currentBatchArrivals.Count, "arrival");
        _currentBatchArrivals.Add(resolvedId);

        int requiredArrivalsForBatch = CurrentBatch.GetRequiredArrivals();
        Debug.Log($"[PrisonerSortModule] Batch {CurrentBatchIndex + 1} arrivals: {_currentBatchArrivals.Count}/{requiredArrivalsForBatch}");
    }

    public void RegisterParticipantFinished(string participantId)
    {
        if (!IsActive || IsComplete || !UsesSessionBatches) return;

        if (CurrentBatch == null)
        {
            Debug.LogWarning($"{name}: Received finish for session mode, but no current batch is bound.");
            return;
        }

        string resolvedId = ResolveParticipantId(participantId, _currentBatchFinished.Count, "finish");
        _currentBatchFinished.Add(resolvedId);

        int requiredFinishesForBatch = CurrentBatch.GetRequiredArrivals();
        Debug.Log($"[PrisonerSortModule] Batch {CurrentBatchIndex + 1} finished: {_currentBatchFinished.Count}/{requiredFinishesForBatch}");

        if (_currentBatchFinished.Count >= requiredFinishesForBatch)
            CompleteCurrentBatch();
    }

    public void DisableInteractables()
    {

    }

    public void SetActiveSession(int sessionIndex)
    {
        activeSessionIndex = sessionIndex;
        ResolveActiveSession();
        SyncSessionVisibility();
    }

    private void ResolveDependencies()
    {
        if (!routeController) routeController = GetComponent<ReactivateRoutes>();
        if (!prisonerRoute) prisonerRoute = FindFirstObjectByType<PrisonerRoute>();
    }

    private void ResolveActiveSession()
    {
        if (sessions == null || sessions.Length == 0)
        {
            ActiveSession = null;
            return;
        }

        if (activeSessionIndex < 0)
            activeSessionIndex = 0;
        else if (activeSessionIndex >= sessions.Length)
            activeSessionIndex = sessions.Length - 1;

        ActiveSession = sessions[activeSessionIndex];
    }

    private void ApplyCurrentBatchBinding()
    {
        CurrentBatch = UsesSessionBatches ? ActiveSession.GetBatch(CurrentBatchIndex) : null;

        if (routeController != null)
            routeController.SetActiveBatch(CurrentBatch);

        if (prisonerRoute != null)
            prisonerRoute.SetActiveBatch(CurrentBatch);
    }

    private void SyncSessionVisibility()
    {
        if (sessions == null) return;

        for (int sessionIndex = 0; sessionIndex < sessions.Length; sessionIndex++)
        {
            PrisonerSortSession session = sessions[sessionIndex];
            bool active = session == ActiveSession;
            if (session?.batches == null) continue;

            for (int batchIndex = 0; batchIndex < session.batches.Length; batchIndex++)
            {
                PrisonerSortBatch batch = session.batches[batchIndex];
                if (batch?.participants == null) continue;

                for (int participantIndex = 0; participantIndex < batch.participants.Length; participantIndex++)
                {
                    GameObject root = batch.participants[participantIndex]?.root;
                    if (root != null)
                        root.SetActive(active);
                }
            }
        }
    }

    private void RegisterLegacyArrival()
    {
        SortedCount++;

        Debug.Log($"[PrisonerSortModule] Arrived: {SortedCount}/{requiredSorted}");

        if (SortedCount >= requiredSorted)
            Complete();
    }

    private void CompleteCurrentBatch()
    {
        if (!UsesSessionBatches || CurrentBatch == null) return;

        routeController?.CompleteCurrentBatch();

        SortedCount++;
        Debug.Log($"[PrisonerSortModule] Batch complete: {SortedCount}/{ActiveSession.GetBatchCount()}");

        CurrentBatchIndex++;

        if (CurrentBatchIndex >= ActiveSession.GetBatchCount())
        {
            if (activeSessionIndex + 1 < sessions.Length)
                activeSessionIndex++;

            routeController?.EndPrisonerSortInteraction();
            Complete();
            return;
        }

        _currentBatchArrivals.Clear();
        _currentBatchFinished.Clear();
        ApplyCurrentBatchBinding();
        routeController?.ResetForBatch();
    }

    private static string ResolveParticipantId(string participantId, int fallbackIndex, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(participantId))
            return participantId;

        return $"{prefix}_{fallbackIndex}";
    }
}
