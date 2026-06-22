using System.Collections.Generic;
using UnityEngine;

public class PrisonerSortModule : InteractionModuleBase
{
    [Header("Cell-Only Batch")]
    [SerializeField] private PrisonerSortParticipant[] participants;
    [SerializeField] private GameObject[] linkedObjects;
    [SerializeField] private CellHoldSelector cellInteractable;
    [SerializeField] private AudioSource selectionAudio;
    [Min(1)]
    [SerializeField] private int requiredFinishedCount = 4;

    public int FinishedCount => _finishedParticipants.Count;
    public bool HasSentBatchToCell { get; private set; }
    public bool UsesSessionBatches => false;

    private readonly HashSet<string> _finishedParticipants = new();

    private void Reset()
    {
        ResolveDependencies();
    }

    public override void Activate()
    {
        base.Activate();

        HasSentBatchToCell = false;
        _finishedParticipants.Clear();
        ResolveDependencies();
        BindParticipantNotifiers();
        ResetParticipants();
        SetBatchActive(true);

        if (cellInteractable != null)
        {
            cellInteractable.Bind(this);
            cellInteractable.ResetSelectionState();
            cellInteractable.SetInteractionEnabled(true);
        }
    }

    public override void Deactivate()
    {
        if (cellInteractable != null)
            cellInteractable.SetInteractionEnabled(false);

        SetBatchActive(false);
        _finishedParticipants.Clear();
        HasSentBatchToCell = false;

        base.Deactivate();
    }

    public void SendBatchToCell()
    {
        if (!IsActive || IsComplete || HasSentBatchToCell)
            return;

        HasSentBatchToCell = true;

        if (cellInteractable != null)
            cellInteractable.SetInteractionEnabled(false);

        if (participants != null)
        {
            for (int i = 0; i < participants.Length; i++)
            {
                PrisonerSortParticipant participant = participants[i];
                if (participant == null)
                    continue;

                participant.SendToCell();
            }
        }

        if (selectionAudio != null)
            selectionAudio.Play();
    }

    public void RegisterParticipantArrived(string participantId)
    {
        if (!IsActive || IsComplete)
            return;

        Debug.Log($"[PrisonerSortModule] Participant arrived: {ResolveParticipantId(participantId)}");
    }

    public void RegisterPrisonerArrived()
    {
        RegisterParticipantArrived(string.Empty);
    }

    public void RegisterParticipantFinished(string participantId)
    {
        if (!IsActive || IsComplete)
            return;

        string resolvedId = ResolveParticipantId(participantId);
        if (!_finishedParticipants.Add(resolvedId))
            return;

        int required = ResolveRequiredFinishedCount();
        Debug.Log($"[PrisonerSortModule] Cell arrivals finished: {_finishedParticipants.Count}/{required}");

        if (_finishedParticipants.Count >= required)
            CompleteCellOnlyBatch();
    }

    private void CompleteCellOnlyBatch()
    {
        if (!IsActive || IsComplete)
            return;

        SetBatchActive(false);

        if (cellInteractable != null)
            cellInteractable.SetInteractionEnabled(false);

        Complete();
    }

    private void ResolveDependencies()
    {
        if (cellInteractable == null)
            cellInteractable = GetComponentInChildren<CellHoldSelector>(true);

        if (cellInteractable == null)
            cellInteractable = FindFirstObjectByType<CellHoldSelector>();

        if (selectionAudio == null)
            selectionAudio = GetComponentInChildren<AudioSource>(true);
    }

    private void BindParticipantNotifiers()
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Length; i++)
            participants[i]?.arrivalNotifier?.Bind(this);
    }

    private void ResetParticipants()
    {
        if (participants == null)
            return;

        for (int i = 0; i < participants.Length; i++)
            participants[i]?.ResetAnimators();
    }

    private void SetBatchActive(bool active)
    {
        if (participants != null)
        {
            for (int i = 0; i < participants.Length; i++)
                participants[i]?.SetActive(active);
        }

        if (linkedObjects == null)
            return;

        for (int i = 0; i < linkedObjects.Length; i++)
        {
            if (linkedObjects[i] != null)
                linkedObjects[i].SetActive(active);
        }
    }

    private int ResolveRequiredFinishedCount()
    {
        if (requiredFinishedCount > 0)
            return requiredFinishedCount;

        return participants != null ? participants.Length : 0;
    }

    private string ResolveParticipantId(string participantId)
    {
        if (!string.IsNullOrWhiteSpace(participantId))
            return participantId;

        return $"participant_{_finishedParticipants.Count}";
    }
}
