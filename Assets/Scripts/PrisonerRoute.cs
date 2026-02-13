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
        index = 0;
    }

    public AudioSource AS;

    public void SelectRoute(string identifier)
    {
        switch(identifier)            
        {
            case "cell":
                ChoiceAnimators[index].SetTrigger("GoToCell");
                PrisonerWalkAnimators[index].SetTrigger("ShouldWalk");
                AS.Play();
                break;
            case "boat":
                ChoiceAnimators[index].SetTrigger("GoToBoat");
                PrisonerWalkAnimators[index].SetTrigger("ShouldWalk");
                AS.Play();
                break;
            case "truck":
                ChoiceAnimators[index].SetTrigger("GoToTruck");
                PrisonerWalkAnimators[index].SetTrigger("ShouldWalk");
                AS.Play();
                break;
        }

        index++;
    }
 
}
