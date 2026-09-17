using UnityEngine;

public class PlanarReflection : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("The XR rig's center eye anchor (head-tracked transform), used instead of Camera.main.")]
    [SerializeField] private Transform CenterEyeAnchor;

    [Header("Reflection Rig")]
    [Tooltip("Empty parent GameObject holding the two reflection stereo cameras. This is what gets moved/rotated, not the cameras directly.")]
    [SerializeField] private Transform ReflectionRig;

    [Tooltip("Reflection camera corresponding to the player's left eye.")]
    [SerializeField] private Camera ReflectionCameraLeft;
    [Tooltip("Reflection camera corresponding to the player's right eye.")]
    [SerializeField] private Camera ReflectionCameraRight;

    [Tooltip("The player's actual left eye camera, whose projection matrix will be copied.")]
    [SerializeField] private Camera PlayerCameraLeft;
    [Tooltip("The player's actual right eye camera, whose projection matrix will be copied.")]
    [SerializeField] private Camera PlayerCameraRight;

    [Header("Reflection Plane")]
    [Tooltip("Empty GameObject marking where the reflective surface sits. Its position is the reflection point, and its local up axis defines the reflection axis.")]
    [SerializeField] private Transform ReflectionPoint;

    [Header("Render Texture")]
    [SerializeField] private RenderTexture ReflectionRenderTexture;
    [SerializeField] private int ReflectionResolution;

    private void Start()
    {
        // Resolution only needs to be set once — VR headset resolution doesn't
        // change at runtime, so no need to Release()/resize every frame.
        Vector2 resolution = new Vector2(Screen.width, Screen.height);

        ReflectionRenderTexture.Release();
        ReflectionRenderTexture.width = Mathf.RoundToInt(resolution.x) * ReflectionResolution / Mathf.RoundToInt(resolution.y);
        ReflectionRenderTexture.height = ReflectionResolution;

        // Make sure both reflection cameras render into the same texture.
        ReflectionCameraLeft.targetTexture = ReflectionRenderTexture;
        ReflectionCameraRight.targetTexture = ReflectionRenderTexture;
    }

    private void LateUpdate()
    {
        // --- Position ---
        // Mirror the tracked head position in the ReflectionPoint's local space,
        // same as before, just using the center eye anchor instead of Camera.main.
        Vector3 localCamPos = ReflectionPoint.InverseTransformPoint(CenterEyeAnchor.position);
        Vector3 localReflectedPos = new Vector3(localCamPos.x, -localCamPos.y, localCamPos.z);
        ReflectionRig.position = ReflectionPoint.TransformPoint(localReflectedPos);

        // --- Rotation ---
        // Same -pitch, yaw, -roll mirror as before, computed relative to the
        // ReflectionPoint's orientation.
        Quaternion localRot = Quaternion.Inverse(ReflectionPoint.rotation) * CenterEyeAnchor.rotation;
        Vector3 localEuler = localRot.eulerAngles;
        Quaternion reflectedLocalRot = Quaternion.Euler(-localEuler.x, localEuler.y, -localEuler.z);
        ReflectionRig.rotation = ReflectionPoint.rotation * reflectedLocalRot;

        // --- Projection matrices ---
        // VR eye cameras use asymmetric frustums, so copy the real projection
        // matrix directly instead of matching fieldOfView.
        ReflectionCameraLeft.projectionMatrix = PlayerCameraLeft.projectionMatrix;
        ReflectionCameraRight.projectionMatrix = PlayerCameraRight.projectionMatrix;
    }
}