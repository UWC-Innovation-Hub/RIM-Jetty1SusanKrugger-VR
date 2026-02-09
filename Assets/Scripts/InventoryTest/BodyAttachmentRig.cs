using UnityEngine;

public class BodyAttachmentRig : MonoBehaviour
{
    [Header("Camera Rig References")]
    public Transform trackingSpace; 
    public Transform centerEyeAnchor;

    [Header("Body Attachment Points")]
    public Transform bodyAttachments;
    public AvatarAttachmentPoint hipAttachment;
    public AvatarAttachmentPoint chestAttachment;
    public AvatarAttachmentPoint beltLeftAttachment;
    public AvatarAttachmentPoint beltRightAttachment;

    [Header("Positioning Configuration")]
    public float hipHeight = -0.4f; // Below head
    public float hipForward = 0.1f; // Slightly forward
    public float chestHeight = -0.2f; // Badge position
    public float beltSideOffset = 0.15f; // Waist position

    void Start()
    {
        SetupBodyAttachments();
    }

    void SetupBodyAttachments()
    {
        if (trackingSpace == null)
        {
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                trackingSpace = cameraRig.trackingSpace;
                centerEyeAnchor = cameraRig.centerEyeAnchor;
            }
        }

        // Create body attachments parent if it doesn't exist
        if (bodyAttachments == null)
        {
            GameObject bodyGO = new GameObject("BodyAttachments");
            bodyAttachments = bodyGO.transform;
            bodyAttachments.SetParent(trackingSpace);
            bodyAttachments.localPosition = Vector3.zero;
            bodyAttachments.localRotation = Quaternion.identity;
        }

        CreateAttachmentPoints();
    }

    void CreateAttachmentPoints()
    {
        // Hip attachment (center)
        if (hipAttachment == null)
        {
            GameObject hipGO = new GameObject("HipAttachment");
            hipGO.transform.SetParent(bodyAttachments);
            hipAttachment = hipGO.AddComponent<AvatarAttachmentPoint>();
            hipAttachment.attachmentType = AvatarAttachmentPoint.AttachmentType.Hip;
        }

        // Chest attachment
        if (chestAttachment == null)
        {
            GameObject chestGO = new GameObject("ChestAttachment");
            chestGO.transform.SetParent(bodyAttachments);
            chestAttachment = chestGO.AddComponent<AvatarAttachmentPoint>();
            chestAttachment.attachmentType = AvatarAttachmentPoint.AttachmentType.Chest;
        }

        // Belt left
        if (beltLeftAttachment == null)
        {
            GameObject beltLeftGO = new GameObject("BeltLeftAttachment");
            beltLeftGO.transform.SetParent(bodyAttachments);
            beltLeftAttachment = beltLeftGO.AddComponent<AvatarAttachmentPoint>();
            beltLeftAttachment.attachmentType = AvatarAttachmentPoint.AttachmentType.Belt;
        }

        // Belt right
        if (beltRightAttachment == null)
        {
            GameObject beltRightGO = new GameObject("BeltRightAttachment");
            beltRightGO.transform.SetParent(bodyAttachments);
            beltRightAttachment = beltRightGO.AddComponent<AvatarAttachmentPoint>();
            beltRightAttachment.attachmentType = AvatarAttachmentPoint.AttachmentType.Belt;
        }
    }

    void Update()
    {
        // Update positions relative to head position
        UpdateBodyPositions();
    }

    // Update body position
    void UpdateBodyPositions()
    {
        if (centerEyeAnchor == null) return;

        // Get head position and forward direction
        Vector3 headPos = centerEyeAnchor.position;
        Vector3 headForward = centerEyeAnchor.forward;
        headForward.y = 0; // Project to horizontal plane
        headForward.Normalize();

        Vector3 headRight = centerEyeAnchor.right;
        headRight.y = 0;
        headRight.Normalize();

        // Position hip attachment
        hipAttachment.transform.position = headPos +
            Vector3.up * hipHeight +
            headForward * hipForward;
        hipAttachment.transform.rotation = Quaternion.LookRotation(headForward);

        // Position chest attachment
        chestAttachment.transform.position = headPos +
            Vector3.up * chestHeight +
            headForward * (hipForward * 0.5f);
        chestAttachment.transform.rotation = Quaternion.LookRotation(headForward);

        // Position belt attachments (left and right side)
        beltLeftAttachment.transform.position = headPos +
            Vector3.up * hipHeight +
            headRight * -beltSideOffset +
            headForward * hipForward;
        beltLeftAttachment.transform.rotation = Quaternion.LookRotation(headForward);

        beltRightAttachment.transform.position = headPos +
            Vector3.up * hipHeight +
            headRight * beltSideOffset +
            headForward * hipForward;
        beltRightAttachment.transform.rotation = Quaternion.LookRotation(headForward);
    }
}