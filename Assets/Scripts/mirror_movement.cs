using UnityEngine;

public class mirror_movement : MonoBehaviour
{
    public Transform playerTarget;
    public Transform mirror;

    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }

    private void LateUpdate()
    {
        RefreshMirrorPose();
    }

    public void RefreshMirrorPose()
    {
        if (playerTarget == null || mirror == null)
            return;

        Vector3 localPlayer = mirror.InverseTransformPoint(playerTarget.position);
        transform.position = mirror.TransformPoint(new Vector3(localPlayer.x, localPlayer.y, -localPlayer.z));

        Vector3 lookatmirror = mirror.TransformPoint(new Vector3(-localPlayer.x, localPlayer.y, localPlayer.z));
        transform.LookAt(lookatmirror);
    }
}
