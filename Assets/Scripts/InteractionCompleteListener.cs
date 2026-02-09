using UnityEngine;

public class InteractionCompleteListener : MonoBehaviour
{
    [SerializeField] private PrisonerSortModule module;

    private void Awake()
    {
        if (!module) module = GetComponent<PrisonerSortModule>();
    }

    private void OnEnable()
    {
        if (module) module.Completed += OnCompleted;
    }

    private void OnDisable()
    {
        if (module) module.Completed -= OnCompleted;
    }

    private void OnCompleted()
    {
        Debug.Log("[InteractionCompleteListener] Prisoner sort interaction COMPLETE!");
    }
}
