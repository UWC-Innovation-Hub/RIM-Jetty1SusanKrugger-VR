using UnityEngine;
using UnityEngine.Serialization;

public class SetNextPrisoner : MonoBehaviour
{
    [SerializeField] private string participantId;
    [FormerlySerializedAs("PrisonerSortModule")]
    [SerializeField] private PrisonerSortModule prisonerSortModule;

    public void Bind(PrisonerSortModule module)
    {
        prisonerSortModule = module;
    }

    public void NotifyArrived()
    {
        prisonerSortModule?.RegisterParticipantArrived(ResolveParticipantId());
    }

    public void NotifyFinished()
    {
        prisonerSortModule?.RegisterParticipantFinished(ResolveParticipantId());
    }

    // Kept so existing animation events and UnityEvents continue to work during migration.
    public void ReactivateRoute()
    {
        NotifyFinished();
    }

    // Kept so existing animation events and UnityEvents continue to work during migration.
    public void IncrementSorter()
    {
        NotifyArrived();
    }

    // Boat routing has been removed from the cell-only prisoner sort interaction.
    public void ActivateBoatDoor()
    {
    }

    // Boat routing has been removed from the cell-only prisoner sort interaction.
    public void CloseBoatDoor()
    {
    }

    private string ResolveParticipantId()
    {
        if (!string.IsNullOrWhiteSpace(participantId))
            return participantId;

        return gameObject.name;
    }
}
