using UnityEngine;

public class PrisonerRoute : MonoBehaviour
{ 
    //public GameObject Prisoner;
    public Animator[] ChoiceAnimators;
    public Animator[] PrisonerWalkAnimators;

    public Animator BoatGateAnimator;
    public string identifier;
    private int index;
    private PrisonerSortSession activeSession;
    private PrisonerSortBatch activeBatch;
    [SerializeField] private RouteHoldSelector routeHoldSelector;
    private string authorizedRouteId;


    private void Awake()
    {
        ResolveHoldSelector();
        ResetForBatch();
    }

    public AudioSource AS;

    public void ResetForBatch()
    {
        index = 0;
        authorizedRouteId = null;

        if (activeSession != null)
        {
            ResetSessionAnimators(activeSession);
            return;
        }

        ResetAnimators(ChoiceAnimators);
        ResetAnimators(PrisonerWalkAnimators);
    }

    public void BindSession(PrisonerSortSession session)
    {
        activeSession = session;
    }

    public void BindHoldSelector(RouteHoldSelector selector)
    {
        routeHoldSelector = selector;
    }

    public void AuthorizeRouteSelection(string routeId)
    {
        authorizedRouteId = routeId;
    }

    public void SetActiveBatch(PrisonerSortBatch batch)
    {
        activeBatch = batch;
    }

    public void SelectRoute(string identifier)
    {
        if (!ConsumeAuthorizedSelection(identifier))
        {
            Debug.Log($"[PrisonerRoute] Ignored unauthorised route selection for '{identifier}'.");
            return;
        }

        if (activeBatch != null)
        {
            SelectBatchRoute(identifier);
            return;
        }

        if (!TryGetAnimatorsForCurrentIndex(out Animator choiceAnimator, out Animator walkAnimator))
        {
            Debug.LogWarning($"{name}: Animator arrays are not ready for index {index}.");
            return;
        }

        switch(identifier)
        {
            case "cell":
                choiceAnimator.SetTrigger("GoToCell");
                break;
            case "boat":
                choiceAnimator.SetTrigger("GoToBoat");
                //BoatGateAnimator.SetTrigger("Open");
                break;
            case "truck":
                choiceAnimator.SetTrigger("GoToTruck");
                break;
            default:
                Debug.LogWarning($"{name}: Unknown route identifier '{identifier}'.");
                return;
        }

        walkAnimator.SetTrigger("ShouldWalk");

        if (AS != null)
            AS.Play();

        index++;
    }

    private void SelectBatchRoute(string identifier)
    {
        if (activeBatch.participants == null || activeBatch.participants.Length == 0)
        {
            Debug.LogWarning($"{name}: Active batch has no participants.");
            return;
        }

        bool triggeredAny = false;

        for (int i = 0; i < activeBatch.participants.Length; i++)
        {
            PrisonerSortParticipant participant = activeBatch.participants[i];
            if (participant == null) continue;

            Animator choiceAnimator = participant.choiceAnimator;
            Animator walkAnimator = participant.walkAnimator;

            if (choiceAnimator == null || walkAnimator == null)
            {
                Debug.LogWarning($"{name}: Participant at index {i} in batch '{activeBatch.batchId}' is missing animator references.");
                continue;
            }

            switch (identifier)
            {
                case "cell":
                    choiceAnimator.SetTrigger("GoToCell");
                    break;
                case "boat":
                    choiceAnimator.SetTrigger("GoToBoat");
                    //BoatGateAnimator.SetTrigger("Open");
                    break;
                case "truck":
                    choiceAnimator.SetTrigger("GoToTruck");
                    break;
                default:
                    Debug.LogWarning($"{name}: Unknown route identifier '{identifier}'.");
                    return;
            }

            walkAnimator.SetTrigger("ShouldWalk");
            triggeredAny = true;
        }

        if (triggeredAny && AS != null)
            AS.Play();
    }

    private bool TryGetAnimatorsForCurrentIndex(out Animator choiceAnimator, out Animator walkAnimator)
    {
        choiceAnimator = null;
        walkAnimator = null;

        if (ChoiceAnimators == null || PrisonerWalkAnimators == null)
            return false;

        if (index < 0 || index >= ChoiceAnimators.Length || index >= PrisonerWalkAnimators.Length)
            return false;

        choiceAnimator = ChoiceAnimators[index];
        walkAnimator = PrisonerWalkAnimators[index];
        return choiceAnimator != null && walkAnimator != null;
    }

    private void ResolveHoldSelector()
    {
        if (routeHoldSelector == null)
            routeHoldSelector = GetComponent<RouteHoldSelector>();

        if (routeHoldSelector == null)
            routeHoldSelector = FindFirstObjectByType<RouteHoldSelector>();
    }

    private bool ConsumeAuthorizedSelection(string routeId)
    {
        ResolveHoldSelector();

        if (routeHoldSelector == null)
            return true;

        if (string.IsNullOrWhiteSpace(authorizedRouteId))
            return false;

        bool matches = string.Equals(authorizedRouteId, routeId, System.StringComparison.OrdinalIgnoreCase);
        if (!matches)
            return false;

        authorizedRouteId = null;
        return true;
    }

    private static void ResetAnimators(Animator[] animators)
    {
        if (animators == null) return;

        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null) continue;

            animator.Rebind();
            animator.Update(0f);
        }
    }

    private static void ResetSessionAnimators(PrisonerSortSession session)
    {
        if (session == null || session.batches == null) return;

        for (int batchIndex = 0; batchIndex < session.batches.Length; batchIndex++)
        {
            PrisonerSortBatch batch = session.batches[batchIndex];
            if (batch == null || batch.participants == null) continue;

            for (int participantIndex = 0; participantIndex < batch.participants.Length; participantIndex++)
            {
                PrisonerSortParticipant participant = batch.participants[participantIndex];
                if (participant == null) continue;

                if (participant.choiceAnimator != null)
                {
                    participant.choiceAnimator.Rebind();
                    participant.choiceAnimator.Update(0f);
                }

                if (participant.walkAnimator != null)
                {
                    participant.walkAnimator.Rebind();
                    participant.walkAnimator.Update(0f);
                }
            }
        }
    }
}
