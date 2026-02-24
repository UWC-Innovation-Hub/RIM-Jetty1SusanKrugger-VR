using UnityEngine;

public class Headlock : MonoBehaviour
{
    public Transform xrRig;
    public float distance = 2f;
    public float heightOffset = 0f;

    void LateUpdate()
    {
        Vector3 flatForward = xrRig.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        transform.position = xrRig.position + (flatForward * distance) + (Vector3.up * heightOffset);

        float yRotation = Quaternion.LookRotation(flatForward).eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
