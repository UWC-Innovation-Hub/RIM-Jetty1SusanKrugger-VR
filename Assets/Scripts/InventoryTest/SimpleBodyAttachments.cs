using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Setup")]
    public bool autoSetup = true;

    [Header("Attachment Points")]
    public GameObject hipSlot;
    public GameObject beltLeftSlot;
    public GameObject beltRightSlot;
    public GameObject chestSlot;

    [Header("Slot Highlight Materials")]
    [SerializeField] private bool instantiateHighlightMaterials = true;
    [SerializeField] private Material hipHighlightMaterial;
    [SerializeField] private Material beltLeftHighlightMaterial;
    [SerializeField] private Material beltRightHighlightMaterial;
    [SerializeField] private Material chestHighlightMaterial;

    private void Start()
    {
        if (autoSetup)
        {
            ConfigureSlots();
        }
    }

    private void ConfigureSlots()
    {
        ConfigureSlot(hipSlot, AvatarAttachmentPoint.AttachmentType.Hip, hipHighlightMaterial);
        ConfigureSlot(beltLeftSlot, AvatarAttachmentPoint.AttachmentType.BeltLeft, beltLeftHighlightMaterial);
        ConfigureSlot(beltRightSlot, AvatarAttachmentPoint.AttachmentType.BeltRight, beltRightHighlightMaterial);
        ConfigureSlot(chestSlot, AvatarAttachmentPoint.AttachmentType.ChestLeft, chestHighlightMaterial);
    }

    private void ConfigureSlot(
        GameObject slot,
        AvatarAttachmentPoint.AttachmentType type,
        Material highlightMaterial)
    {
        if (slot == null)
        {
            Debug.LogWarning($"{name}: Missing {type} attachment slot.", this);
            return;
        }

        AvatarAttachmentPoint attachPoint = slot.GetComponent<AvatarAttachmentPoint>();
        if (attachPoint == null)
        {
            Debug.LogWarning($"{name}: {slot.name} is missing AvatarAttachmentPoint.", slot);
            return;
        }

        attachPoint.attachmentType = type;
        attachPoint.ConfigureHighlightMaterial(highlightMaterial, instantiateHighlightMaterials);
    }
}
