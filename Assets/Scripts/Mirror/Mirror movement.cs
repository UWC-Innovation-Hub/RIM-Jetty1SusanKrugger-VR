using UnityEngine;

public class VRMirrorPerEyeAxis : MonoBehaviour
{
    [Header("Eye Anchors")]
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;

    [Header("Mirror Cameras")]
    public Transform leftMirrorCamera;
    public Transform rightMirrorCamera;

    [Header("Mirror Plane")]
    public Transform mirrorPlane;

    public enum MirrorAxis { X, Y, Z }
    public MirrorAxis mirrorAxis = MirrorAxis.Z;

    void LateUpdate()
    {
        if (!leftEyeAnchor || !rightEyeAnchor || !mirrorPlane) return;

        Vector3 planePos = mirrorPlane.position;
        Vector3 planeNormal = GetMirrorNormal();

        // Mirror positions
        leftMirrorCamera.position = ReflectPosition(leftEyeAnchor.position, planePos, planeNormal);
        rightMirrorCamera.position = ReflectPosition(rightEyeAnchor.position, planePos, planeNormal);

        // Lock rotation to mirror (looking "out" of the mirror)
        Quaternion mirrorRotation = Quaternion.LookRotation(planeNormal, mirrorPlane.up);

        leftMirrorCamera.rotation = mirrorRotation;
        rightMirrorCamera.rotation = mirrorRotation;
    }

    Vector3 ReflectPosition(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        Vector3 toPoint = point - planePoint;
        Vector3 reflected = Vector3.Reflect(toPoint, planeNormal);
        return reflected + planePoint;
    }

    Vector3 GetMirrorNormal()
    {
        switch (mirrorAxis)
        {
            case MirrorAxis.X:
                return mirrorPlane.right;
            case MirrorAxis.Y:
                return mirrorPlane.up;
            case MirrorAxis.Z:
                return mirrorPlane.forward;
            default:
                return mirrorPlane.forward;
        }
    }
}