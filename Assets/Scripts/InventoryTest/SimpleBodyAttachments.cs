using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Auto-Setup")]
    public bool autoSetup = true;

    [Header("Position Configuration")]
    [SerializeField] float hipHeight = -0.4f;      // moved up ~15cm
    [SerializeField] float hipForward = 0.05f;      // pushed forward so visible when looking down
    [SerializeField] float hipSideOffset = -0.12f;  // left side for baton
    [SerializeField] float chestHeight = -0.2f;     // Moved up ~10cm
    [SerializeField] float chestForward = 0.07f;     // pushed forward
    [SerializeField] float chestSideOffset = -0.08f; // left side of chest
    [SerializeField] float beltSideOffset = 0.16f;  // slightly tighter

    [Header("Attachment Points (Auto-Created)")]
    public GameObject hipSlot;
    public GameObject beltLeftSlot;
    public GameObject beltRightSlot;
    public GameObject chestSlot;

    [Header("Runtime Slot Highlight Materials")]
    [SerializeField] private bool instantiateHighlightMaterials = true;
    [SerializeField] private Material hipHighlightMaterial;
    [SerializeField] private Material beltLeftHighlightMaterial;
    [SerializeField] private Material beltRightHighlightMaterial;
    [SerializeField] private Material chestHighlightMaterial;

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
        attachPoint.ConfigureHighlightMaterial(GetHighlightMaterialForType(type), instantiateHighlightMaterials);

        Debug.Log($"Created {name}");
    }

    private Material GetHighlightMaterialForType(AvatarAttachmentPoint.AttachmentType type)
    {
        switch (type)
        {
            case AvatarAttachmentPoint.AttachmentType.Hip:
                return hipHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.BeltLeft:
                return beltLeftHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.BeltRight:
                return beltRightHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.ChestLeft:
                return chestHighlightMaterial;
            default:
                return null;
        }
    }

    void Update()
    {
        if (centerEye == null) return;

        // Get head position and direction
        Vector3 headPos = centerEye.position;
        Vector3 headForward = Vector3.ProjectOnPlane(centerEye.forward, Vector3.up);

        // Guard before normalizing — fallback if head is looking straight up/down
        if (headForward.sqrMagnitude < 0.0001f)
            headForward = Vector3.ProjectOnPlane(centerEye.right, Vector3.up);

        if (headForward.sqrMagnitude < 0.0001f)
            return;

        headForward.Normalize();
        Vector3 headRight = Vector3.Cross(Vector3.up, headForward).normalized;

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
