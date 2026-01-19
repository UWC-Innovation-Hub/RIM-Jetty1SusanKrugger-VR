using UnityEngine;

public class PrisonerRoute : MonoBehaviour
{
    public GameObject Prisoner;
    public Animator ChoiceAnimator;
    public Animator PrisonerWalkAnimator;
    public string identifier;

    public AudioSource AS;

    public void Awake()
    {
 
    }


    public void SelectRoute(string identifier)
    {
        switch(identifier)            
        {
            case "cell":
                ChoiceAnimator.SetTrigger("GoToCell");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                AS.Play();
                break;
            case "boat":
                ChoiceAnimator.SetTrigger("GoToBoat");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                AS.Play();
                break;
            case "truck":
                ChoiceAnimator.SetTrigger("GoToTruck");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                AS.Play();
                break;
        }
    }

 
}
