using UnityEngine;

public sealed class AnimatorTriggerRelay : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "OpenCell";
    [SerializeField] private string[] resetTriggers = new string[0];

    public void Trigger()
    {
        if (!animator || string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        if (resetTriggers != null)
        {
            foreach (string resetTrigger in resetTriggers)
            {
                if (!string.IsNullOrWhiteSpace(resetTrigger))
                {
                    animator.ResetTrigger(resetTrigger);
                }
            }
        }

        animator.SetTrigger(triggerName);
    }
}
