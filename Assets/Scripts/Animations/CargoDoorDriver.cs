using UnityEngine;

public class CargoDoorDriver : MonoBehaviour
{
    public Animator animator;
    public string openTrigger = "Open", closeTrigger = "Close";
    public void Open() { if (animator) animator.SetTrigger(openTrigger); }
    public void Close() { if (animator) animator.SetTrigger(closeTrigger); }
}
