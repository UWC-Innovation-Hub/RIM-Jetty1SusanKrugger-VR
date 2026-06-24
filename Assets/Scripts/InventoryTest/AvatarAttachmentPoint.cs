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
    [SerializeField] private MeshRenderer visualizer;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private bool instantiateHighlightMaterial = true;
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float idleEmissionStrength = 0f;
    [SerializeField] private float guidedEmissionStrength = 2f;
    [SerializeField] private bool lerpGuidanceEmission = true;
    [SerializeField] private float guidanceLerpInHoldDuration = 0.2f;
    [SerializeField] private float guidanceLerpInDuration = 0.35f;
    [SerializeField] private float guidanceLerpOutDuration = 0.25f;

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
    private bool ownsVisualizerMaterial;
    private bool _isGuidanceLerping;
    private float _guidanceLerpStart;
    private float _guidanceLerpTarget;
    private float _guidanceLerpDuration;
    private float _guidanceLerpElapsed;
    private float _guidanceHoldRemaining;

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
        Material visualMat = GetVisualizerMaterial();
        if (visualMat == null || !visualMat.HasProperty(emissionStrengthProperty))
        {
            return;
        }

        float target = highlighted ? guidedEmissionStrength : idleEmissionStrength;
        if (!lerpGuidanceEmission)
        {
            _isGuidanceLerping = false;
            _guidanceHoldRemaining = 0f;
            visualMat.SetFloat(emissionStrengthProperty, target);
            return;
        }

        float current;
        float duration;
        if (highlighted)
        {
            // Ensure we always sit at pure idle before the lerp begins.
            visualMat.SetFloat(emissionStrengthProperty, idleEmissionStrength);
            current = idleEmissionStrength;
            duration = guidanceLerpInDuration;
            _guidanceHoldRemaining = Mathf.Max(0f, guidanceLerpInHoldDuration);
        }
        else
        {
            current = visualMat.GetFloat(emissionStrengthProperty);
            duration = guidanceLerpOutDuration;
            _guidanceHoldRemaining = 0f;
        }

        if (Mathf.Approximately(current, target))
        {
            _isGuidanceLerping = false;
            return;
        }

        _guidanceLerpTarget = target;
        _guidanceLerpStart = current;
        _guidanceLerpDuration = Mathf.Max(0.0001f, duration);
        _guidanceLerpElapsed = 0f;
        _isGuidanceLerping = true;
    }

    void Start()
    {
        SetupSnapZone();
    }

    void Update()
    {
        TickGuidanceLerp();
    }

    void SetupSnapZone()
    {
        snapZone = GetComponent<SphereCollider>();
        if (snapZone == null)
        {
            Debug.LogWarning($"{name}: Missing SphereCollider for {attachmentType} snap zone.", this);
        }
        else
        {
            snapZone.radius = snapRadius;
            snapZone.isTrigger = true;
        }

        ResolveVisualizer();
        if (visualizer != null)
        {
            ApplyVisualizerMaterial();
            ConfigureVisualizerRenderer();
            SetGuidanceHighlighted(false);
        }
        else
        {
            Debug.LogWarning($"{name}: Missing editor-authored visualizer for {attachmentType} snap zone.", this);
        }

        Debug.Log($"Setup {attachmentType} snap zone at {transform.position}");
    }

    private void ResolveVisualizer()
    {
        if (visualizer != null)
        {
            return;
        }

        Transform child = transform.Find("SnapZoneVisualizer");
        if (child != null)
        {
            visualizer = child.GetComponent<MeshRenderer>();
        }

        if (visualizer == null)
        {
            visualizer = GetComponentInChildren<MeshRenderer>(true);
        }
    }

    private void ConfigureVisualizerRenderer()
    {
        if (visualizer == null)
        {
            return;
        }

        visualizer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visualizer.receiveShadows = false;
    }

    private void ApplyVisualizerMaterial()
    {
        if (visualizer == null)
        {
            return;
        }

        if (ownsVisualizerMaterial && visualizer.material != null)
        {
            DestroyOwnedMaterial(visualizer.material);
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

    private Material GetVisualizerMaterial()
    {
        if (visualizer == null)
        {
            return null;
        }

        return ownsVisualizerMaterial ? visualizer.material : visualizer.sharedMaterial;
    }

    private static void DestroyOwnedMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

    private void TickGuidanceLerp()
    {
        if (!_isGuidanceLerping)
        {
            return;
        }

        Material visualMat = GetVisualizerMaterial();
        if (visualMat == null || !visualMat.HasProperty(emissionStrengthProperty))
        {
            _isGuidanceLerping = false;
            _guidanceHoldRemaining = 0f;
            return;
        }

        if (_guidanceHoldRemaining > 0f)
        {
            _guidanceHoldRemaining -= Time.deltaTime;
            visualMat.SetFloat(emissionStrengthProperty, idleEmissionStrength);
            return;
        }

        _guidanceLerpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_guidanceLerpElapsed / Mathf.Max(0.0001f, _guidanceLerpDuration));
        float next = Mathf.Lerp(_guidanceLerpStart, _guidanceLerpTarget, t);
        visualMat.SetFloat(emissionStrengthProperty, next);

        if (t >= 1f || Mathf.Approximately(next, _guidanceLerpTarget))
        {
            _isGuidanceLerping = false;
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

        item.transform.SetParent(transform, false);
        item.transform.localPosition = localPositionOffset + item.GetEquippedPositionOffset();
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

    public static bool IsCompatible(AttachmentType itemType, AttachmentType slotType)
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
            DestroyOwnedMaterial(visualizer.material);
        }
    }

    // Debug visualization in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }

}
