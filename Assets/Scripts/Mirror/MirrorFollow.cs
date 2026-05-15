using UnityEngine;

[DisallowMultipleComponent]
public class MirrorFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mirrorCamera;
    [SerializeField] private Transform mirrorSurface;
    [SerializeField] private Transform sourceHead;
    [SerializeField] private Camera sourceCamera;

    [Header("Reflection Setup")]
    [Tooltip("Flip this if the mirror appears to reflect from the wrong side.")]
    [SerializeField] private bool flipSurfaceNormal;
    [SerializeField] private bool copySourceFov = true;

    [Header("Quest Performance")]
    [SerializeField] private bool applyLowCostCameraDefaults = true;
    [SerializeField, Min(1)] private int renderEveryNFrames = 1;
    [SerializeField] private float mirrorFarClip = 40f;
    [SerializeField] private bool overrideCullingMask;
    [SerializeField] private LayerMask mirrorCullingMask = ~0;

    private void Reset()
    {
        if (!mirrorCamera)
            mirrorCamera = GetComponent<Camera>();

        if (!mirrorSurface && transform.parent != null)
            mirrorSurface = transform.parent;

        TryAutoAssignSource();
    }

    private void Awake()
    {
        TryAutoAssignSource();

        if (applyLowCostCameraDefaults && mirrorCamera)
            ApplyQuestDefaults();
    }

    private void LateUpdate()
    {
        if (!mirrorCamera || !mirrorSurface)
            return;

        if (!sourceHead)
            TryAutoAssignSource();

        if (!sourceHead)
            return;

        if (copySourceFov && sourceCamera)
            mirrorCamera.fieldOfView = sourceCamera.fieldOfView;

        UpdateMirroredPose();

        if (renderEveryNFrames <= 1)
        {
            if (!mirrorCamera.enabled)
                mirrorCamera.enabled = true;
            return;
        }

        bool shouldRenderThisFrame = (Time.frameCount % renderEveryNFrames) == 0;
        if (mirrorCamera.enabled != shouldRenderThisFrame)
            mirrorCamera.enabled = shouldRenderThisFrame;
    }

    private void TryAutoAssignSource()
    {
        if (!sourceCamera && Camera.main)
            sourceCamera = Camera.main;

        if (!sourceHead && sourceCamera)
            sourceHead = sourceCamera.transform;
    }

    private void ApplyQuestDefaults()
    {
        mirrorCamera.allowHDR = false;
        mirrorCamera.allowMSAA = false;
        mirrorCamera.stereoTargetEye = StereoTargetEyeMask.None;
        mirrorCamera.farClipPlane = Mathf.Max(1f, mirrorFarClip);

        if (overrideCullingMask)
            mirrorCamera.cullingMask = mirrorCullingMask;
    }

    private void UpdateMirroredPose()
    {
        Vector3 normal = mirrorSurface.forward;
        if (flipSurfaceNormal)
            normal = -normal;

        normal.Normalize();
        Vector3 planePoint = mirrorSurface.position;

        Vector3 sourcePosition = sourceHead.position;
        float distanceToPlane = Vector3.Dot(normal, sourcePosition - planePoint);
        Vector3 mirroredPosition = sourcePosition - (2f * distanceToPlane * normal);

        Vector3 mirroredForward = Vector3.Reflect(sourceHead.forward, normal);
        Vector3 mirroredUp = Vector3.Reflect(sourceHead.up, normal);

        transform.SetPositionAndRotation(mirroredPosition, Quaternion.LookRotation(mirroredForward, mirroredUp));
    }
}
