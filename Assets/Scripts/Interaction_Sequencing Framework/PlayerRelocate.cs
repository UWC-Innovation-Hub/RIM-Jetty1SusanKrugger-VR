using UnityEngine;

public class PlayerRelocate : MonoBehaviour
{
    [Header("Object To Move")]
    public GameObject targetObject;

    [Header("Target Position")]
    public Vector3 targetPosition;

    // Call this function to relocate the assigned object
    public void Relocate()
    {
        if (targetObject != null)
        {
            targetObject.transform.position = targetPosition;
        }
        else
        {
            Debug.LogWarning("No target object assigned.");
        }
    }
}