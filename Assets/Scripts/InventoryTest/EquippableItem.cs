using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

[RequireComponent(typeof(Rigidbody))]
public class EquippableItem : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName = "Item";
    public ItemType itemType;
    public AvatarAttachmentPoint.AttachmentType compatibleAttachmentPoint;

    [Header("Grab Settings")]
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private Rigidbody rb;

    [Header("Visual Settings")]
    public Vector3 equippedScale = Vector3.one;
    public Vector3 handHeldScale = Vector3.one;

    public enum ItemType
    {
        Key,
        Baton,
        Badge,
        WalkieTalkie,
        Generic
    }

    private bool isEquipped = false;
    private bool isBeingGrabbed = false;
    private Vector3 originalScale;
    private Vector3 originalLocalScale; // Store local scale

    void Awake()
    {
        // Store the WORLD scale, not local scale
        originalScale = transform.lossyScale;
        originalLocalScale = transform.localScale;

        // Get components
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Subscribe to grab events
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += OnGrabEvent;
        }
    }

    void OnGrabEvent(PointerEvent pointerEvent)
    {
        switch (pointerEvent.Type)
        {
            case PointerEventType.Select:
                OnGrabbed();
                break;
            case PointerEventType.Unselect:
                OnReleased();
                break;
        }
    }

    void OnGrabbed()
    {
        isBeingGrabbed = true;

        // If was equipped, unequip it
        if (isEquipped)
        {
            AvatarAttachmentPoint attachPoint = GetComponentInParent<AvatarAttachmentPoint>();
            if (attachPoint != null)
            {
                transform.SetParent(null);
                attachPoint.UnequipItem();
            }
            isEquipped = false;
        }

        // Restore original scale when grabbed
        transform.localScale = originalLocalScale;

        // Enable physics when held
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
        }

        Debug.Log($"Grabbed {itemName}");
    }

    void OnReleased()
    {
        isBeingGrabbed = false;

        // Enable gravity when dropped (if not equipped)
        if (rb != null && !isEquipped)
        {
            rb.useGravity = true;
        }

        Debug.Log($"Released {itemName}");
    }

    public void OnEquippedToSlot()
    {
        isEquipped = true;

        // CRITICAL: Set local scale AFTER parenting
        // Use original local scale, not the equipped scale multiplier
        transform.localScale = originalLocalScale;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log($"{itemName} equipped - local scale: {transform.localScale}");
    }

    public bool IsEquipped() => isEquipped;

    public bool IsBeingGrabbed() => isBeingGrabbed;

    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }
}