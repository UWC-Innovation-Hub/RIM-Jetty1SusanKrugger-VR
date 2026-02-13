using UnityEngine;
using Oculus.Interaction;

public class AvatarAttachmentPoint : MonoBehaviour
{
    [Header("Attachment Configuration")]
    public AttachmentType attachmentType;

    [Header("Snap Zone Settings")]
    public float snapRadius = 0.08f;
    
    [Header("Positioning")]
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(1f, 1f, 0f, 0.08f); // Sphere Color yellow before insertion
    public Color hoverColor = new Color(0f, 1f, 0f, 0.15f);  // Color green indicates correct attachment point
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private bool instantiateHighlightMaterial = true;
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float idleEmissionStrength = 0f;
    [SerializeField] private float guidedEmissionStrength = 2f;

    // Attachment Points
    public enum AttachmentType
    {
        Hip,
        Belt,
        Chest,
        Shoulder,
        Back,
        Waist,
        BeltLeft,
        BeltRight,
        ChestLeft
    }

    // Variables to enable attachment
    private EquippableItem currentEquippedItem;
    private EquippableItem nearbyItem;
    private SphereCollider snapZone;
    private MeshRenderer visualizer;
    private bool ownsVisualizerMaterial;

    public void ConfigureHighlightMaterial(Material material, bool instantiateMaterial)
    {
        highlightMaterial = material;
        instantiateHighlightMaterial = instantiateMaterial;

        // If the visualizer already exists, apply immediately.
        if (visualizer != null)
        {
            ApplyVisualizerMaterial();
        }
    }

    public void SetGuidanceHighlighted(bool highlighted)
    {
        if (visualizer == null)
        {
            return;
        }

        Material visualMat = ownsVisualizerMaterial ? visualizer.material : visualizer.sharedMaterial;
        if (visualMat == null || !visualMat.HasProperty(emissionStrengthProperty))
        {
            return;
        }

        visualMat.SetFloat(
            emissionStrengthProperty,
            highlighted ? guidedEmissionStrength : idleEmissionStrength
        );
    }

    void Start()
    {
        SetupSnapZone();
    }

    void SetupSnapZone()
    {
        // Create or get sphere collider
        snapZone = GetComponent<SphereCollider>();
        if (snapZone == null)
        {
            snapZone = gameObject.AddComponent<SphereCollider>();
        }
        snapZone.radius = snapRadius;
        snapZone.isTrigger = true;

        // Create visual indicator
        CreateVisualIndicator();

        Debug.Log($"Setup {attachmentType} snap zone at {transform.position}");
    }

    void CreateVisualIndicator()
    {
        // Create a sphere to show the snap zone
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "SnapZoneVisualizer";
        sphere.transform.SetParent(transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * snapRadius * 0.4f;

        // Remove the collider (we only want visual)
        Destroy(sphere.GetComponent<Collider>());

        visualizer = sphere.GetComponent<MeshRenderer>();
        ApplyVisualizerMaterial();
        visualizer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visualizer.receiveShadows = false;
        SetGuidanceHighlighted(false);
    }

    private void ApplyVisualizerMaterial()
    {
        if (visualizer == null)
        {
            return;
        }

        if (ownsVisualizerMaterial && visualizer.material != null)
        {
            Destroy(visualizer.material);
        }

        Material mat;
        if (highlightMaterial != null)
        {
            if (instantiateHighlightMaterial)
            {
                mat = new Material(highlightMaterial);
                ownsVisualizerMaterial = true;
                visualizer.material = mat;
            }
            else
            {
                mat = highlightMaterial;
                ownsVisualizerMaterial = false;
                visualizer.sharedMaterial = mat;
            }
        }
        else
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            // If URP shader not found, try Standard
            if (mat.shader.name == "Hidden/InternalErrorShader")
            {
                mat = new Material(Shader.Find("Standard"));
            }

            // Set to transparent rendering mode
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;

            // Set color with transparency
            if (mat.HasProperty("_Color"))
            {
                mat.color = normalColor;
            }

            ownsVisualizerMaterial = true;
            visualizer.material = mat;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{attachmentType}] Trigger enter: {other.gameObject.name}");

        // Check if the object has an EquippableItem component
        EquippableItem item = other.GetComponentInParent<EquippableItem>();

        if (item == null)
        {
            item = other.GetComponent<EquippableItem>();
        }

        if (item != null)
        {
            Debug.Log($"[{attachmentType}] Found item: {item.itemName}, IsEquipped: {item.IsEquipped()}, CompatibleWith: {item.compatibleAttachmentPoint}, ThisSlot: {attachmentType}");

            if (!item.IsEquipped() && IsCompatible(item.compatibleAttachmentPoint, attachmentType))
            {
                nearbyItem = item;

                // Visual feedback - change color
                if (visualizer != null)
                {
                    Material visualMat = ownsVisualizerMaterial ? visualizer.material : visualizer.sharedMaterial;
                    if (visualMat != null && visualMat.HasProperty("_Color"))
                    {
                        visualMat.color = hoverColor;
                    }
                }

                Debug.Log($"Item {item.itemName} COMPATIBLE with {attachmentType} slot"); //Log info
            }
            else
            {
                Debug.Log($"Item {item.itemName} NOT compatible - already equipped or wrong slot type");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // If item is nearby and not being grabbed, snap it
        if (nearbyItem != null && !nearbyItem.IsBeingGrabbed())
        {
            Debug.Log($"[{attachmentType}] Snapping {nearbyItem.itemName}");
            SnapItem(nearbyItem);
        }
    }

    void OnTriggerExit(Collider other)
    {
        EquippableItem item = other.GetComponentInParent<EquippableItem>();
        if (item == null)
        {
            item = other.GetComponent<EquippableItem>();
        }

        // Only proceed if we found an item AND it matches our nearby item
        if (item != null && item == nearbyItem)
        {
            Debug.Log($"[{attachmentType}] Item {item.itemName} left trigger zone");
            nearbyItem = null;

            // Reset visual feedback
            if (visualizer != null)
            {
                Material visualMat = ownsVisualizerMaterial ? visualizer.material : visualizer.sharedMaterial;
                if (visualMat != null && visualMat.HasProperty("_Color"))
                {
                    visualMat.color = normalColor;
                }
            }
        }
    }

    void SnapItem(EquippableItem item)
    {
        if (currentEquippedItem != null)
        {
            Debug.Log($"[{attachmentType}] Already has item equipped");
            return;
        }

        currentEquippedItem = item;

        // Parent to this attachment point
        item.transform.SetParent(transform);

        // Apply positioning
        item.transform.localPosition = localPositionOffset;
        item.transform.localRotation = Quaternion.Euler(localRotationOffset) * item.GetEquippedRotationOffset();

        // Notify the item it's been equipped
        item.OnEquippedToSlot();

        // Notify inventory manager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemEquipped(attachmentType, item);
        }

        // Hide visualizer when item is equipped
        if (visualizer != null)
        {
            visualizer.enabled = false;
        }

        nearbyItem = null;

        Debug.Log($"✅ Successfully snapped {item.itemName} to {attachmentType}");
    }

    public void UnequipItem()
    {
        if (currentEquippedItem == null)
            return;

        Debug.Log($"[{attachmentType}] Unequipping {currentEquippedItem.itemName}");

        EquippableItem item = currentEquippedItem;
        currentEquippedItem = null; // Clear first to prevent re-entrant calls

        item.transform.SetParent(null);
        item.NotifyUnequipped(); // Clears isEquipped flag and re-enables physics on item

        // Show visualizer again
        if (visualizer != null)
            visualizer.enabled = true;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemUnequipped(attachmentType);
    }

    public bool HasItemEquipped()
    {
        return currentEquippedItem != null;
    }

    private static bool IsCompatible(AttachmentType itemType, AttachmentType slotType)
    {
        if (itemType == slotType)
        {
            return true;
        }

        if (itemType == AttachmentType.Belt &&
            (slotType == AttachmentType.BeltLeft || slotType == AttachmentType.BeltRight))
        {
            return true;
        }

        if (itemType == AttachmentType.BeltLeft && slotType == AttachmentType.Belt)
        {
            return true;
        }

        if (itemType == AttachmentType.BeltRight && slotType == AttachmentType.Belt)
        {
            return true;
        }

        if (itemType == AttachmentType.Chest && slotType == AttachmentType.ChestLeft)
        {
            return true;
        }

        if (itemType == AttachmentType.ChestLeft && slotType == AttachmentType.Chest)
        {
            return true;
        }

        return false;
    }

    void OnDestroy()
    {
        if (ownsVisualizerMaterial && visualizer != null && visualizer.material != null)
        {
            Destroy(visualizer.material);
        }
    }

    // Debug visualization in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }
}
