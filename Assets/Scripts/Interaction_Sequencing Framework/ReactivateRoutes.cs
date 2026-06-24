using UnityEngine;

public class ReactivateRoutes : MonoBehaviour
{
    public GameObject[] Prisoners;
    public GameObject[] Routes;
    public Material[] RouteMats;
    public HighlightExit Grabbed;
    [SerializeField] private RouteHoldSelector routeSelection;
    public Animator[] PrisonerWalkAnimator;
    public GameObject DistanceHandGrabInteractor;

    [Header("Batch Reset")]
    [SerializeField] private bool reactivateAllPrisonersOnReset = true;
    [SerializeField] private int firstActivePrisonerIndex = 0;
    [SerializeField] private bool restorePrisonerTransformsOnReset = true;

    private int identifier;
    private Vector3[] initialLocalPositions;
    private Quaternion[] initialLocalRotations;
    private Vector3[] initialLocalScales;



    //Increment prisoner for interaction end point detection.
    public PrisonerSortModule PrisonerSortModule;
    private PrisonerSortSession activeSession;
    private PrisonerSortBatch activeBatch;


    private void Awake()
    {
        CacheInitialPrisonerTransforms();
        identifier = 0;
        ResolveRouteSelection();
    }

    public void ResetForBatch()
    {
        ResolveRouteSelection();

        if (activeBatch == null)
            identifier = 0;

        SetRoutesActive(true);

        routeSelection?.ResetSelectionState();

        SetDistanceHandGrabInteractorActive(true);

        if (Grabbed != null)
            Grabbed.grabbed = false;

        ResetMats();

        if (activeBatch != null)
        {
            SetBatchParticipantsActive(activeBatch, true);
            SetBatchLinkedObjectsActive(activeBatch, true);
            return;
        }

        ResetPrisonersToStartState();
    }


    public void ReactivateRoute()
    {
        ResolveRouteSelection();

        if (activeBatch != null)
        {
            ResetForBatch();
            return;
        }

        SetRoutesActive(true);

        SetDistanceHandGrabInteractorActive(true);

        routeSelection?.ResetSelectionState();

        ResetMats();


        //Grabbed is a single variable now that HighlightExit is a singleton referred to by each distancegrabbable
        if (Grabbed != null)
            Grabbed.grabbed = false;
        //foreach(HighlightExit dest in GrabbedAr)
        //{
        //    dest.grabbed = false;
        //}



        ////Trigger idle animation
        ////Is this needed? As once the prisoner has walked, they should deactivate?
        //PrisonerWalkAnimator[identifier].SetTrigger("ShouldIdle");


        //Deactivate Current Prisoner
        if (Prisoners == null || identifier < 0 || identifier >= Prisoners.Length)
        {
            Debug.LogWarning($"{name}: Prisoner index {identifier} is out of range.");
            return;
        }

        if (Prisoners[identifier] != null)
            Prisoners[identifier].SetActive(false);

        identifier++;
    }


    public void EndPrisonerSortInteraction()
    {
        ResolveRouteSelection();
        SetRoutesActive(false);

        routeSelection?.SetInteractionEnabled(false);

        SetDistanceHandGrabInteractorActive(false);

        ResetMats();

        if (Grabbed != null)
            Grabbed.grabbed = false;

        if (activeBatch != null)
            return;

        if (Prisoners == null) return;

        foreach (GameObject go in Prisoners)
        {
            if (go != null)
                go.SetActive(false);
        }
        //Trigger idle animation
        //PrisonerWalkAnimator.SetTrigger("ShouldIdle");

    }



    //function to increment prisoner sort condition
    public void IncrementSorter()
    {
        if (PrisonerSortModule != null)
            PrisonerSortModule.RegisterPrisonerArrived();
    }

    public void BindSession(PrisonerSortSession session)
    {
        activeSession = session;
    }

    public void SetActiveBatch(PrisonerSortBatch batch)
    {
        activeBatch = batch;
    }

    public void CompleteCurrentBatch()
    {
        if (activeBatch == null) return;

        SetBatchParticipantsActive(activeBatch, false);
        SetBatchLinkedObjectsActive(activeBatch, false);
        ResolveRouteSelection();
        routeSelection?.ResetSelectionState();
        ResetMats();

        if (Grabbed != null)
            Grabbed.grabbed = false;
    }


    private void OnApplicationQuit()
    {
        ResetMats();
    }

    private void ResolveRouteSelection()
    {
        if (routeSelection == null)
            routeSelection = GetComponentInChildren<RouteHoldSelector>(true);

        if (routeSelection == null)
            routeSelection = FindFirstObjectByType<RouteHoldSelector>();
    }

    public void SetDistanceHandGrabInteractorActive(bool active)
    {
        if (DistanceHandGrabInteractor != null)
            DistanceHandGrabInteractor.SetActive(active);
    }


    public void ResetMats()
    {
        if (RouteMats == null) return;

        float idleStrength = PrisonerSortModule != null && PrisonerSortModule.IsActive ? 0.0f : 0f;

        foreach (Material mat in RouteMats)
        {
            if (mat == null) continue;
            mat.SetColor("_EmissionColor", new Color(1f, 0.8509f, 0.2980f));
            mat.SetFloat("_EmissionStrength", idleStrength);
        }
    }

    private void CacheInitialPrisonerTransforms()
    {
        if (Prisoners == null)
        {
            initialLocalPositions = null;
            initialLocalRotations = null;
            initialLocalScales = null;
            return;
        }

        int count = Prisoners.Length;
        initialLocalPositions = new Vector3[count];
        initialLocalRotations = new Quaternion[count];
        initialLocalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            if (Prisoners[i] == null) continue;

            Transform t = Prisoners[i].transform;
            initialLocalPositions[i] = t.localPosition;
            initialLocalRotations[i] = t.localRotation;
            initialLocalScales[i] = t.localScale;
        }
    }

    private void ResetPrisonersToStartState()
    {
        if (Prisoners == null || Prisoners.Length == 0) return;

        int firstIndex = Mathf.Clamp(firstActivePrisonerIndex, 0, Prisoners.Length - 1);

        for (int i = 0; i < Prisoners.Length; i++)
        {
            GameObject prisoner = Prisoners[i];
            if (prisoner == null) continue;

            if (restorePrisonerTransformsOnReset &&
                initialLocalPositions != null &&
                initialLocalRotations != null &&
                initialLocalScales != null &&
                i < initialLocalPositions.Length)
            {
                Transform t = prisoner.transform;
                t.localPosition = initialLocalPositions[i];
                t.localRotation = initialLocalRotations[i];
                t.localScale = initialLocalScales[i];
            }

            bool shouldBeActive = reactivateAllPrisonersOnReset || i == firstIndex;
            prisoner.SetActive(shouldBeActive);
        }
    }

    private void SetRoutesActive(bool active)
    {
        if (Routes == null) return;

        foreach (GameObject go in Routes)
        {
            if (go != null)
                go.SetActive(active);
        }
    }

    private static void SetBatchParticipantsActive(PrisonerSortBatch batch, bool active)
    {
        if (batch == null || batch.participants == null) return;

        for (int i = 0; i < batch.participants.Length; i++)
        {
            PrisonerSortParticipant participant = batch.participants[i];
            if (participant?.root == null) continue;
            participant.root.SetActive(active);
        }
    }

    private static void SetBatchLinkedObjectsActive(PrisonerSortBatch batch, bool active)
    {
        if (batch?.linkedObjects == null) return;

        for (int i = 0; i < batch.linkedObjects.Length; i++)
        {
            GameObject linkedObject = batch.linkedObjects[i];
            if (linkedObject != null)
                linkedObject.SetActive(active);
        }
    }

}
