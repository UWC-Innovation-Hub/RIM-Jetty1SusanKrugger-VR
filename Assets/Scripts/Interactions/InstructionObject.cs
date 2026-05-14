using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class InstructionObject : MonoBehaviour
{
    public InstructionManager instructionManager;

    private bool hasInteracted = false;

    public void OnInteract(SelectEnterEventArgs args)
    {
        if (hasInteracted)
        {
            return;
        }

        hasInteracted = true;
        instructionManager.OnObjectInteracted();

        Debug.Log("Pointer Clicked!");
    }
}
