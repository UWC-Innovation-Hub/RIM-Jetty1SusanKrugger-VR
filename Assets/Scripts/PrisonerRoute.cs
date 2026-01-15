using UnityEngine;

public class PrisonerRoute : MonoBehaviour
{
    public GameObject Prisoner;
    public Animator ChoiceAnimator;
    public Animator PrisonerWalkAnimator;
    public string identifier; 



    public void SelectRoute(string identifier)
    {
        switch(identifier)            
        {
            case "cell":
                ChoiceAnimator.SetTrigger("GoToCell");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                break;
            case "boat":
                ChoiceAnimator.SetTrigger("GoToBoat");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                break;
            case "truck":
                ChoiceAnimator.SetTrigger("GoToTruck");
                PrisonerWalkAnimator.SetTrigger("ShouldWalk");
                break;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
