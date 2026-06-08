using UnityEngine;

public class SimpleBodyAttachments : MonoBehaviour
{
    [Header("Auto-Setup")]
    public bool autoSetup = true;

    [Header("Spawn Point References")]
    public Transform hipSpawnPoint;
    public Transform beltLeftSpawnPoint;
    public Transform beltRightSpawnPoint;
    public Transform chestSpawnPoint;

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

    void Start()
    {
        if (autoSetup)
        {
            AutoSetup();
        }
    }

    void AutoSetup()
    {
        if (hipSpawnPoint == null || beltLeftSpawnPoint == null ||
            beltRightSpawnPoint == null || chestSpawnPoint == null)
        {
            Debug.LogError("One or more spawn points are not assigned!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager instance not found! Make sure it exists in the scene.");
            return;
        }

        CreateSlot(ref hipSlot, "HipSlot", hipSpawnPoint, AvatarAttachmentPoint.AttachmentType.Hip);
        CreateSlot(ref beltLeftSlot, "BeltLeftSlot", beltLeftSpawnPoint, AvatarAttachmentPoint.AttachmentType.BeltLeft);
        CreateSlot(ref beltRightSlot, "BeltRightSlot", beltRightSpawnPoint, AvatarAttachmentPoint.AttachmentType.BeltRight);
        CreateSlot(ref chestSlot, "ChestSlot", chestSpawnPoint, AvatarAttachmentPoint.AttachmentType.ChestLeft);

        Debug.Log("Body attachment system initialized successfully!");
    }

    void CreateSlot(ref GameObject slot, string name, Transform spawnPoint, AvatarAttachmentPoint.AttachmentType type)
    {
        slot = new GameObject(name);
        slot.transform.SetParent(spawnPoint);
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localRotation = Quaternion.identity;

        AvatarAttachmentPoint attachPoint = slot.AddComponent<AvatarAttachmentPoint>();
        attachPoint.attachmentType = type;
        attachPoint.snapRadius = 0.15f;
        attachPoint.ConfigureHighlightMaterial(GetHighlightMaterialForType(type), instantiateHighlightMaterials);

        // Register with InventoryManager
        InventoryManager.Instance.attachmentPoints.Add(attachPoint);

        Debug.Log($"Created {name} as child of {spawnPoint.name} and registered with InventoryManager");
    }

    private Material GetHighlightMaterialForType(AvatarAttachmentPoint.AttachmentType type)
    {
        switch (type)
        {
            case AvatarAttachmentPoint.AttachmentType.Hip: return hipHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.BeltLeft: return beltLeftHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.BeltRight: return beltRightHighlightMaterial;
            case AvatarAttachmentPoint.AttachmentType.ChestLeft: return chestHighlightMaterial;
            default: return null;
        }
    }
}