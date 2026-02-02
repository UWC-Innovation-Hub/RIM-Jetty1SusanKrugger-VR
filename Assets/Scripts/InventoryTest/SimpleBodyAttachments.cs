using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Auto-Setup")]
    public bool autoSetup = true;

    [Header("Position Configuration")]
    public float hipHeight = -0.6f;      // Lower - 60cm below head
    public float hipForward = 0.0f;      // Don't push forward, stay at body
    public float chestHeight = -0.3f;    // 30cm below head
    public float chestForward = 0.05f;   // Just slightly forward
    public float beltSideOffset = 0.2f;  // Further to sides

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

        // Create attachment slots - they'll follow the head
        CreateSlot(ref hipSlot, "HipSlot", AvatarAttachmentPoint.AttachmentType.Hip);
        CreateSlot(ref beltLeftSlot, "BeltLeftSlot", AvatarAttachmentPoint.AttachmentType.Belt);
        CreateSlot(ref beltRightSlot, "BeltRightSlot", AvatarAttachmentPoint.AttachmentType.Belt);
        CreateSlot(ref chestSlot, "ChestSlot", AvatarAttachmentPoint.AttachmentType.Chest);

        Debug.Log("Body attachment system initialized successfully!");
    }

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
                headForward * hipForward;
            hipSlot.transform.rotation = bodyRotation;
        }

        // Update ChestSlot position
        if (chestSlot != null)
        {
            chestSlot.transform.position = headPos +
                Vector3.up * chestHeight +
                headForward * chestForward;
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