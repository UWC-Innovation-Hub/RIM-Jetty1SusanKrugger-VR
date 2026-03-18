using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Settings")]
    public int intStep = 3;

    [Header("References")]
    public EmissionTextureRotator progress;

    private int interactionCount = 0;

    public void RegisterInteraction()
    {
        interactionCount++;

        if (interactionCount % intStep == 0)
        {
            progress.AdvanceStep();
        }
    }
}
