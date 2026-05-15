using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ProgressObject : MonoBehaviour
{
    public ProgressTracker progressTracker;
    public float cooldown = 0.5f;

    private float lastInteraction;

    public void OnInteract()
    {
       if (Time.time - lastInteraction < cooldown)
        {
            return;
        }

       lastInteraction = Time.time;
        progressTracker.RegisterInteraction();
    }
}
