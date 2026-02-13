using UnityEngine;

[DisallowMultipleComponent]
public sealed class BodyAttachmentRig : MonoBehaviour
{
    [Header("Camera Rig References")]
    [SerializeField] private Transform trackingSpace;
    [SerializeField] private Transform centerEyeAnchor;

    [Header("Body Attachment Points")]
    [SerializeField] private Transform bodyAttachments;
    [SerializeField] private AvatarAttachmentPoint hipAttachment;
    [SerializeField] private AvatarAttachmentPoint chestAttachment;
    [SerializeField] private AvatarAttachmentPoint beltLeftAttachment;
    [SerializeField] private AvatarAttachmentPoint beltRightAttachment;

    [Header("Positioning")]
    [SerializeField] private bool updatePositions = true;
    [SerializeField] private float hipHeight = -0.4f;
    [SerializeField] private float hipForward = 0.1f;
    [SerializeField] private float hipSideOffset = -0.12f;
    [SerializeField] private float chestHeight = -0.2f;
    [SerializeField] private float chestForward = 0.05f;
    [SerializeField] private float chestSideOffset = -0.08f;
    [SerializeField] private float beltSideOffset = 0.15f;

    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        if (!updatePositions)
        {
            return;
        }

        if (!initialized)
        {
            Initialize();
        }

        if (centerEyeAnchor == null)
        {
            return;
        }

        UpdateBodyPositions();
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        ResolveCameraRig();
        EnsureBodyAttachmentsRoot();
        EnsureAttachment(ref hipAttachment, "HipAttachment", AvatarAttachmentPoint.AttachmentType.Hip);
        EnsureAttachment(ref chestAttachment, "ChestAttachment", AvatarAttachmentPoint.AttachmentType.ChestLeft);
        EnsureAttachment(ref beltLeftAttachment, "BeltLeftAttachment", AvatarAttachmentPoint.AttachmentType.BeltLeft);
        EnsureAttachment(ref beltRightAttachment, "BeltRightAttachment", AvatarAttachmentPoint.AttachmentType.BeltRight);
    }

    private void ResolveCameraRig()
    {
        if (trackingSpace != null && centerEyeAnchor != null)
        {
            return;
        }

        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig == null)
        {
            return;
        }

        if (trackingSpace == null)
        {
            trackingSpace = cameraRig.trackingSpace;
        }

        if (centerEyeAnchor == null)
        {
            centerEyeAnchor = cameraRig.centerEyeAnchor;
        }
    }

    private void EnsureBodyAttachmentsRoot()
    {
        if (bodyAttachments != null)
        {
            return;
        }

        GameObject bodyGO = new GameObject("BodyAttachments");
        bodyAttachments = bodyGO.transform;

        Transform parent = trackingSpace != null ? trackingSpace : transform;
        bodyAttachments.SetParent(parent, false);
    }

    private void EnsureAttachment(
        ref AvatarAttachmentPoint attachment,
        string name,
        AvatarAttachmentPoint.AttachmentType type)
    {
        if (attachment != null)
        {
            return;
        }

        GameObject attachmentGO = new GameObject(name);
        attachmentGO.transform.SetParent(bodyAttachments, false);
        attachment = attachmentGO.AddComponent<AvatarAttachmentPoint>();
        attachment.attachmentType = type;
    }

    private void UpdateBodyPositions()
    {
        Vector3 headForward = Vector3.ProjectOnPlane(centerEyeAnchor.forward, Vector3.up);
        if (headForward.sqrMagnitude < 0.0001f)
        {
            Vector3 fallbackForward = trackingSpace != null ? trackingSpace.forward : transform.forward;
            headForward = Vector3.ProjectOnPlane(fallbackForward, Vector3.up);
        }

        if (headForward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        headForward.Normalize();
        Vector3 headRight = Vector3.Cross(Vector3.up, headForward).normalized;
        Vector3 headPos = centerEyeAnchor.position;

        Quaternion facing = Quaternion.LookRotation(headForward, Vector3.up);

        Vector3 hipPos = headPos +
            Vector3.up * hipHeight +
            headForward * hipForward +
            headRight * hipSideOffset;
        SetAttachmentTransform(hipAttachment, hipPos, facing);

        Vector3 chestPos = headPos + Vector3.up * chestHeight +
            headForward * chestForward +
            headRight * chestSideOffset;
        SetAttachmentTransform(chestAttachment, chestPos, facing);

        Vector3 beltOffset = headRight * beltSideOffset;
        SetAttachmentTransform(beltLeftAttachment, hipPos - beltOffset, facing);
        SetAttachmentTransform(beltRightAttachment, hipPos + beltOffset, facing);
    }

    private static void SetAttachmentTransform(
        AvatarAttachmentPoint attachment,
        Vector3 position,
        Quaternion rotation)
    {
        if (attachment == null)
        {
            return;
        }

        attachment.transform.SetPositionAndRotation(position, rotation);
    }
}
