using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Auto-Setup")]
    public bool autoSetup = true;

    [Header("Reference Points")]
    [SerializeField] private Transform hipReference;
    [SerializeField] private Transform beltLeftReference;
    [SerializeField] private Transform beltRightReference;
    [SerializeField] private Transform chestReference;

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

    private void Start()
    {
        if (autoSetup)
        {
            AutoSetup();
        }
    }

    private void AutoSetup()
    {
        // Find camera rig
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();

        if (rig == null)
        {
            Debug.LogError("OVRCameraRig not found!");
            return;
        }

        trackingSpace = rig.trackingSpace;

        // Create attachment slots
        CreateSlot(ref hipSlot, "HipSlot", AvatarAttachmentPoint.AttachmentType.Hip);
        CreateSlot(ref beltLeftSlot, "BeltLeftSlot", AvatarAttachmentPoint.AttachmentType.BeltLeft);
        CreateSlot(ref beltRightSlot, "BeltRightSlot", AvatarAttachmentPoint.AttachmentType.BeltRight);
        CreateSlot(ref chestSlot, "ChestSlot", AvatarAttachmentPoint.AttachmentType.ChestLeft);

        Debug.Log("Body attachment system initialized successfully!");
    }

    private void CreateSlot(ref GameObject slot, string name, AvatarAttachmentPoint.AttachmentType type)
    {
        slot = new GameObject(name);

        if (trackingSpace != null)
        {
            slot.transform.SetParent(trackingSpace);
        }

        slot.transform.localPosition = Vector3.zero;
        slot.transform.localRotation = Quaternion.identity;

        AvatarAttachmentPoint attachPoint = slot.AddComponent<AvatarAttachmentPoint>();

        attachPoint.attachmentType = type;
        attachPoint.snapRadius = 0.15f;
        attachPoint.ConfigureHighlightMaterial(
            GetHighlightMaterialForType(type),
            instantiateHighlightMaterials);

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

    private void Update()
    {
        UpdateSlot(hipSlot, hipReference);
        UpdateSlot(beltLeftSlot, beltLeftReference);
        UpdateSlot(beltRightSlot, beltRightReference);
        UpdateSlot(chestSlot, chestReference);
    }

    private void UpdateSlot(GameObject slot, Transform reference)
    {
        if (slot == null || reference == null)
            return;

        // Match world position and rotation exactly
        slot.transform.position = reference.position;
        slot.transform.rotation = reference.rotation;
    }
}