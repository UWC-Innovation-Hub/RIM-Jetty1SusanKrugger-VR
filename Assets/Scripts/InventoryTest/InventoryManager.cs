using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Attachment Points")]
    public List<AvatarAttachmentPoint> attachmentPoints = new List<AvatarAttachmentPoint>();

    [Header("Events")]
    public System.Action<AvatarAttachmentPoint.AttachmentType, EquippableItem> OnItemEquippedEvent;
    public System.Action<AvatarAttachmentPoint.AttachmentType> OnItemUnequippedEvent;

    private Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem> equippedItems
        = new Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnItemEquipped(AvatarAttachmentPoint.AttachmentType attachmentType,
                               EquippableItem item)
    {
        // If something already equipped, force unequip
        if (equippedItems.ContainsKey(attachmentType))
        {
            UnequipItem(attachmentType);
        }

        equippedItems[attachmentType] = item;
        OnItemEquippedEvent?.Invoke(attachmentType, item);

        Debug.Log($"[Inventory] Equipped {item.itemName} to {attachmentType}");
    }

    public void OnItemUnequipped(AvatarAttachmentPoint.AttachmentType attachmentType)
    {
        if (equippedItems.ContainsKey(attachmentType))
        {
            equippedItems.Remove(attachmentType);
            OnItemUnequippedEvent?.Invoke(attachmentType);

            Debug.Log($"[Inventory] Unequipped item from {attachmentType}");
        }
    }

    public void UnequipItem(AvatarAttachmentPoint.AttachmentType attachmentType)
    {
        if (equippedItems.TryGetValue(attachmentType, out var item))
        {
            // Find the attachment point and detach item
            AvatarAttachmentPoint attachPoint = attachmentPoints.Find(
                ap => ap.attachmentType == attachmentType
            );

            if (attachPoint != null && item != null)
            {
                // Detach from parent
                item.transform.SetParent(null);

                // Re-enable physics
                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }
    }

    public bool HasItemEquipped(AvatarAttachmentPoint.AttachmentType attachmentType)
    {
        return equippedItems.ContainsKey(attachmentType) &&
               equippedItems[attachmentType] != null;
    }

    public EquippableItem GetEquippedItem(AvatarAttachmentPoint.AttachmentType attachmentType)
    {
        return equippedItems.TryGetValue(attachmentType, out var item) ? item : null;
    }

    public Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem> GetAllEquippedItems()
    {
        return new Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem>(equippedItems);
    }
}