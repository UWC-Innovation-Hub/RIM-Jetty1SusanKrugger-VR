using UnityEngine;

public class PrisonerRoute : MonoBehaviour
{
    //public GameObject Prisoner;
    public Animator[] ChoiceAnimators;
    public Animator[] PrisonerWalkAnimators;
    public string identifier;
    private int index;


    private void Awake()
    {
        ResetForBatch();
    }

    public AudioSource AS;

    public void ResetForBatch()
    {
        index = 0;
        ResetAnimators(ChoiceAnimators);
        ResetAnimators(PrisonerWalkAnimators);
    }

    public void SelectRoute(string identifier)
    {
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
}
