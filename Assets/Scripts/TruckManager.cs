using UnityEngine;

public class TruckManager : MonoBehaviour
{
    public Animator animator;
    public string arriveTrigger = "Arrive", leaveTrigger = "Leave";
    public void Arrive() { if (animator) animator.SetTrigger(arriveTrigger); }
    public void Leave() { if (animator) animator.SetTrigger(leaveTrigger); }
}
