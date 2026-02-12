using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Auto-Setup")]
    public bool autoSetup = true;

    [Header("Position Configuration")]
    float hipHeight = -0.4f;      // moved up ~15cm
    float hipForward = 0.05f;      // pushed forward so visible when looking down
    float hipSideOffset = -0.12f;  // left side for baton
    float chestHeight = -0.2f;     // Moved up ~10cm
    float chestForward = 0.07f;     // pushed forward
    float chestSideOffset = -0.08f; // left side of chest
    float beltSideOffset = 0.16f;  // slightly tighter

    [Header("Attachment Points (Auto-Created)")]
    public GameObject hipSlot;
    public GameObject beltLeftSlot;
    public GameObject beltRightSlot;
    public GameObject chestSlot;

    private Transform trackingSpace; 
    private Transform centerEye;

    void Start()
    {
        if (autoSetup)
        {
            AutoSetup();
        }
    }

    void AutoSetup()
    {
        // Find camera rig
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        if (rig == null)
        {
            Debug.LogError("OVRCameraRig not found!");
            return;
        }

        trackingSpace = rig.trackingSpace;
        centerEye = rig.centerEyeAnchor;

        if (centerEye == null)
        {
            Debug.LogError("CenterEyeAnchor not found!");
            return;
        }

        // Attachment slots that  will follow the head
        CreateSlot(ref hipSlot, "HipSlot", AvatarAttachmentPoint.AttachmentType.Hip);
        CreateSlot(ref beltLeftSlot, "BeltLeftSlot", AvatarAttachmentPoint.AttachmentType.BeltLeft);
        CreateSlot(ref beltRightSlot, "BeltRightSlot", AvatarAttachmentPoint.AttachmentType.BeltRight);
        CreateSlot(ref chestSlot, "ChestSlot", AvatarAttachmentPoint.AttachmentType.ChestLeft);

        Debug.Log("Body attachment system initialized successfully!");
    }
    // Create slots 
    void CreateSlot(ref GameObject slot, string name, AvatarAttachmentPoint.AttachmentType type)
    {
        slot = new GameObject(name);
        slot.transform.SetParent(trackingSpace);

        // Start at tracking space origin, will be positioned in Update
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localRotation = Quaternion.identity;

        // Add attachment point component
        AvatarAttachmentPoint attachPoint = slot.AddComponent<AvatarAttachmentPoint>();
        attachPoint.attachmentType = type;
        attachPoint.snapRadius = 0.15f;

        Debug.Log($"Created {name}");
    }

    void Update()
    {
        if (centerEye == null) return;

        // Get head position and direction
        Vector3 headPos = centerEye.position;
        Vector3 headForward = centerEye.forward;
        headForward.y = 0; // Project to horizontal plane
        headForward.Normalize();

        Vector3 headRight = centerEye.right;
        headRight.y = 0;
        headRight.Normalize();

        // Only update if we have a valid forward direction
        if (headForward.magnitude < 0.1f)
        {
            headForward = Vector3.forward;
        }

        Quaternion bodyRotation = Quaternion.LookRotation(headForward);

        // Update HipSlot position
        if (hipSlot != null)
        {
            hipSlot.transform.position = headPos +
                Vector3.up * hipHeight +
                headForward * hipForward +
                headRight * hipSideOffset;
            hipSlot.transform.rotation = bodyRotation;
        }

        // Update ChestSlot position
        if (chestSlot != null)
        {
            chestSlot.transform.position = headPos +
                Vector3.up * chestHeight +
                headForward * chestForward +
                headRight * chestSideOffset;
            chestSlot.transform.rotation = bodyRotation;
        }

        // Update BeltLeftSlot position
        if (beltLeftSlot != null)
        {
            beltLeftSlot.transform.position = headPos +
                Vector3.up * hipHeight +
                headForward * hipForward +
                headRight * -beltSideOffset; // Left side (negative right)
            beltLeftSlot.transform.rotation = bodyRotation;
        }

        // Update BeltRightSlot position
        if (beltRightSlot != null)
        {
            beltRightSlot.transform.position = headPos +
                Vector3.up * hipHeight +
                headForward * hipForward +
                headRight * beltSideOffset; // Right side (positive right)
            beltRightSlot.transform.rotation = bodyRotation;
        }
    }
}
