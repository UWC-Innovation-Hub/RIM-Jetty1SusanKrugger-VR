using UnityEngine;

public class SK_controller : MonoBehaviour
{
    [SerializeField] private Animator SKAnimator;

    private const string ShouldArriveTrigger = "ShouldArrive";
    private const string ShouldLeaveTrigger = "ShouldLeave";


    public void SK_Arrive()
    {
        if (!SKAnimator) return;        
        SKAnimator.SetTrigger(ShouldArriveTrigger);
    }


    public void SK_Leave()
    {
        if (!SKAnimator) return;        
        SKAnimator.SetTrigger(ShouldLeaveTrigger);
    }    
}
