using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class InventoryModule : InteractionModuleBase
{
    private enum OrderedMatchMode
    {
        ExactReference,
        ItemType
    }

    [System.Serializable]
    private class ItemHighlightBinding
    {
        public EquippableItem item;
        public Material[] highlightMaterials;
    }

    [System.Serializable]
    private class ItemAttachmentBinding
    {
        public EquippableItem item;
        public AvatarAttachmentPoint[] targetAttachmentPoints;
    }

    [Header("Wiring")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Completion Rule")]
    [SerializeField] private int requiredEquippedCount = 4;

    [Header("Order Enforcement")]
    [SerializeField] private bool enforceOrder = true;
    [Tooltip("Ordered list of scene items. For your flow: Keys, WalkieTalkie, Baton, Badge.")]
    [SerializeField] private List<EquippableItem> orderedItems = new List<EquippableItem>();
    [SerializeField] private OrderedMatchMode orderedMatchMode = OrderedMatchMode.ItemType;
    [SerializeField] private bool rejectOutOfOrderEquips = true;
    [SerializeField] private bool lockCorrectlyPlacedItems = true;

    [Header("Highlight (Material References)")]
    [SerializeField] private bool useHighlight = true;
    [SerializeField] private List<ItemHighlightBinding> highlightBindings = new List<ItemHighlightBinding>();
    [SerializeField] private string emissionStrengthProperty = "_EmissionStrength";
    [SerializeField] private float highlightedEmissionStrength = 2f;
    [SerializeField] private float idleEmissionStrength = 0f;
    [SerializeField] private bool lerpHighlightIn = true;
    [SerializeField] private float highlightLerpInHoldDuration = 0.2f;
    [SerializeField] private float highlightLerpDuration = 0.35f;
    [SerializeField] private bool lerpHighlightOut = true;
    [SerializeField] private float highlightOutLerpDuration = 0.25f;

    [Header("Attachment Point Guidance")]
    [SerializeField] private bool highlightExpectedAttachmentPoints = true;
    [SerializeField] private bool autoFindAttachmentPoints = true;
    [SerializeField] private List<ItemAttachmentBinding> itemAttachmentBindings = new List<ItemAttachmentBinding>();
    [SerializeField] private List<AvatarAttachmentPoint> attachmentPoints = new List<AvatarAttachmentPoint>();

    public int CorrectPlacedCount { get; private set; }
    public int CurrentStepIndex { get; private set; }

    private readonly HashSet<EquippableItem> _acceptedItems = new HashSet<EquippableItem>();

    private EquippableItem _highlightedItem;
    private bool _subscribed;
    private readonly List<Material> _highlightLerpMaterials = new List<Material>();
    private readonly Dictionary<Material, float> _highlightLerpTargets = new Dictionary<Material, float>();
    private readonly Dictionary<Material, float> _highlightLerpStarts = new Dictionary<Material, float>();
    private readonly Dictionary<Material, float> _highlightLerpDurations = new Dictionary<Material, float>();
    private readonly Dictionary<Material, float> _highlightLerpElapsed = new Dictionary<Material, float>();
    private readonly Dictionary<Material, float> _highlightLerpHolds = new Dictionary<Material, float>();

    private void Update()
    {
        TickHighlightLerpIn();
    }

    public override void Activate()
    {
        base.Activate();

        ResolveInventoryManager();

        _acceptedItems.Clear();
        CorrectPlacedCount = 0;
        CurrentStepIndex = 0;

        SubscribeInventoryEvents();
        ResolveAttachmentPoints();
        SetAllConfiguredHighlights(idleEmissionStrength);
        RebuildProgressFromInventory();
        UpdateHighlight();
        UpdateAttachmentPointHighlights();
        TryCompleteIfReady();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        UnsubscribeInventoryEvents();
        ClearHighlightLerp();
        ClearCurrentHighlight();
        ClearAttachmentPointHighlights();
        SetAllConfiguredHighlights(idleEmissionStrength);
    }

    private void OnItemEquipped(AvatarAttachmentPoint.AttachmentType slot, EquippableItem item)
    {
        if (!IsActive || IsComplete || item == null)
        {
            return;
        }

        if (!enforceOrder)
        {
            if (_acceptedItems.Add(item))
            {
                CorrectPlacedCount = _acceptedItems.Count;
            }

            TryCompleteIfReady();
            return;
        }

        int target = GetTargetCount();
        if (CurrentStepIndex >= target)
        {
            return;
        }

        EquippableItem expectedItem = GetExpectedItem(CurrentStepIndex);
        if (expectedItem == null)
        {
            Debug.LogWarning($"{name}: Ordered inventory list has a missing item at step {CurrentStepIndex}.");
            return;
        }

        if (!IsMatch(item, expectedItem))
        {
            if (rejectOutOfOrderEquips && inventoryManager != null)
            {
                inventoryManager.UnequipItem(slot);
            }
            return;
        }

        if (_acceptedItems.Contains(item))
        {
            return;
        }

        _acceptedItems.Add(item);
        CorrectPlacedCount = _acceptedItems.Count;
        CurrentStepIndex = CorrectPlacedCount;

        if (lockCorrectlyPlacedItems && ShouldLockItem(item))
        {
            SetItemLocked(item, true);
        }

        UpdateHighlight();
        UpdateAttachmentPointHighlights();
        TryCompleteIfReady();
    }

    private void OnItemUnequipped(AvatarAttachmentPoint.AttachmentType _)
    {
        if (!IsActive || IsComplete)
        {
            return;
        }

        RebuildProgressFromInventory();
        UpdateHighlight();
        UpdateAttachmentPointHighlights();
    }

    private void ResolveInventoryManager()
    {
        if (inventoryManager != null)
        {
            return;
        }

        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning($"{name}: InventoryModule could not find InventoryManager.");
        }
    }

    private void SubscribeInventoryEvents()
    {
        if (_subscribed || inventoryManager == null)
        {
            return;
        }

        inventoryManager.OnItemEquippedEvent += OnItemEquipped;
        inventoryManager.OnItemUnequippedEvent += OnItemUnequipped;
        _subscribed = true;
    }

    private void UnsubscribeInventoryEvents()
    {
        if (!_subscribed || inventoryManager == null)
        {
            return;
        }

        inventoryManager.OnItemEquippedEvent -= OnItemEquipped;
        inventoryManager.OnItemUnequippedEvent -= OnItemUnequipped;
        _subscribed = false;
    }

    private void RebuildProgressFromInventory()
    {
        _acceptedItems.Clear();
        CorrectPlacedCount = 0;
        CurrentStepIndex = 0;

        if (inventoryManager == null)
        {
            return;
        }

        Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem> equipped =
            inventoryManager.GetAllEquippedItems();

        if (!enforceOrder)
        {
            int target = GetTargetCount();
            foreach (EquippableItem equippedItem in equipped.Values)
            {
                if (equippedItem == null)
                {
                    continue;
                }

                _acceptedItems.Add(equippedItem);
                if (_acceptedItems.Count >= target)
                {
                    break;
                }
            }

            CorrectPlacedCount = _acceptedItems.Count;
            CurrentStepIndex = CorrectPlacedCount;
            TryCompleteIfReady();
            return;
        }

        int orderedTarget = GetTargetCount();
        for (int i = 0; i < orderedTarget; i++)
        {
            EquippableItem expected = GetExpectedItem(i);
            if (expected == null)
            {
                break;
            }

            if (!ContainsExpectedItem(equipped, expected))
            {
                break;
            }

            _acceptedItems.Add(expected);
        }

        CorrectPlacedCount = _acceptedItems.Count;
        CurrentStepIndex = CorrectPlacedCount;
        TryCompleteIfReady();
    }

    private int GetTargetCount()
    {
        int target = Mathf.Max(1, requiredEquippedCount);

        if (!enforceOrder)
        {
            return target;
        }

        if (orderedItems == null || orderedItems.Count == 0)
        {
            return target;
        }

        return Mathf.Min(target, orderedItems.Count);
    }

    private EquippableItem GetExpectedItem(int stepIndex)
    {
        if (orderedItems == null || stepIndex < 0 || stepIndex >= orderedItems.Count)
        {
            return null;
        }

        return orderedItems[stepIndex];
    }

    private void TryCompleteIfReady()
    {
        if (CorrectPlacedCount >= GetTargetCount())
        {
            Complete();
        }
    }

    private void UpdateHighlight()
    {
        if (!useHighlight || !enforceOrder)
        {
            ClearCurrentHighlight();
            return;
        }

        EquippableItem nextItem = GetExpectedItem(CurrentStepIndex);
        if (_highlightedItem == nextItem)
        {
            return;
        }

        ClearCurrentHighlight();
        _highlightedItem = nextItem;

        if (_highlightedItem != null)
        {
            SetItemHighlight(_highlightedItem, true);
        }
    }

    private void ResolveAttachmentPoints()
    {
        if (!highlightExpectedAttachmentPoints || !autoFindAttachmentPoints)
        {
            return;
        }

        if (attachmentPoints.Count > 0)
        {
            return;
        }

        AvatarAttachmentPoint[] points = FindObjectsByType<AvatarAttachmentPoint>(FindObjectsSortMode.None);
        if (points == null || points.Length == 0)
        {
            return;
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                attachmentPoints.Add(points[i]);
            }
        }
    }

    private void UpdateAttachmentPointHighlights()
    {
        if (!highlightExpectedAttachmentPoints)
        {
            return;
        }

        ResolveAttachmentPoints();
        ClearAttachmentPointHighlights();

        if (!IsActive || !enforceOrder)
        {
            return;
        }

        EquippableItem expectedItem = GetExpectedItem(CurrentStepIndex);
        if (expectedItem == null)
        {
            return;
        }

        for (int i = 0; i < attachmentPoints.Count; i++)
        {
            AvatarAttachmentPoint point = attachmentPoints[i];
            if (point == null)
            {
                continue;
            }

            if (ShouldHighlightPointForExpectedItem(expectedItem, point))
            {
                point.SetGuidanceHighlighted(true);
            }
        }
    }

    private bool ShouldHighlightPointForExpectedItem(EquippableItem expectedItem, AvatarAttachmentPoint point)
    {
        AvatarAttachmentPoint[] preferredPoints = GetExplicitAttachmentPoints(expectedItem);
        if (preferredPoints != null && preferredPoints.Length > 0)
        {
            for (int i = 0; i < preferredPoints.Length; i++)
            {
                if (preferredPoints[i] == point)
                {
                    return true;
                }
            }

            return false;
        }

        return IsAttachmentTypeCompatible(expectedItem.compatibleAttachmentPoint, point.attachmentType);
    }

    private AvatarAttachmentPoint[] GetExplicitAttachmentPoints(EquippableItem expectedItem)
    {
        if (expectedItem == null || itemAttachmentBindings == null)
        {
            return null;
        }

        for (int i = 0; i < itemAttachmentBindings.Count; i++)
        {
            ItemAttachmentBinding binding = itemAttachmentBindings[i];
            if (binding == null || binding.item == null)
            {
                continue;
            }

            if (IsMatch(binding.item, expectedItem))
            {
                return binding.targetAttachmentPoints;
            }
        }

        return null;
    }

    private void ClearAttachmentPointHighlights()
    {
        if (!highlightExpectedAttachmentPoints || attachmentPoints == null)
        {
            return;
        }

        for (int i = 0; i < attachmentPoints.Count; i++)
        {
            AvatarAttachmentPoint point = attachmentPoints[i];
            if (point != null)
            {
                point.SetGuidanceHighlighted(false);
            }
        }
    }

    private void ClearCurrentHighlight()
    {
        if (_highlightedItem == null)
        {
            return;
        }

        SetItemHighlight(_highlightedItem, false);
        _highlightedItem = null;
    }

    private void SetItemHighlight(EquippableItem item, bool highlighted)
    {
        if (item == null || !useHighlight)
        {
            return;
        }

        Material[] materials = GetHighlightMaterials(item);
        if (materials == null || materials.Length == 0)
        {
            return;
        }

        float targetStrength = highlighted ? highlightedEmissionStrength : idleEmissionStrength;
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null)
            {
                continue;
            }

            if (!mat.HasProperty(emissionStrengthProperty))
            {
                continue;
            }

            if (highlighted && lerpHighlightIn)
            {
                QueueHighlightLerp(
                    mat,
                    highlightedEmissionStrength,
                    highlightLerpDuration,
                    resetToIdleBeforeLerp: true,
                    holdBeforeLerp: highlightLerpInHoldDuration
                );
            }
            else if (!highlighted && lerpHighlightOut)
            {
                QueueHighlightLerp(
                    mat,
                    idleEmissionStrength,
                    highlightOutLerpDuration,
                    resetToIdleBeforeLerp: false,
                    holdBeforeLerp: 0f
                );
            }
            else
            {
                RemoveHighlightLerp(mat);
                mat.SetFloat(emissionStrengthProperty, targetStrength);
            }
        }
    }

    private Material[] GetHighlightMaterials(EquippableItem item)
    {
        if (item == null || highlightBindings == null)
        {
            return null;
        }

        for (int i = 0; i < highlightBindings.Count; i++)
        {
            ItemHighlightBinding binding = highlightBindings[i];
            if (binding == null || binding.item != item)
            {
                continue;
            }

            return binding.highlightMaterials;
        }

        return null;
    }

    private void SetAllConfiguredHighlights(float strength)
    {
        if (!useHighlight || highlightBindings == null)
        {
            return;
        }

        ClearHighlightLerp();

        for (int i = 0; i < highlightBindings.Count; i++)
        {
            ItemHighlightBinding binding = highlightBindings[i];
            if (binding == null || binding.highlightMaterials == null)
            {
                continue;
            }

            for (int m = 0; m < binding.highlightMaterials.Length; m++)
            {
                Material mat = binding.highlightMaterials[m];
                if (mat == null || !mat.HasProperty(emissionStrengthProperty))
                {
                    continue;
                }

                mat.SetFloat(emissionStrengthProperty, strength);
            }
        }
    }

    private void TickHighlightLerpIn()
    {
        if (_highlightLerpMaterials.Count == 0)
        {
            return;
        }

        for (int i = _highlightLerpMaterials.Count - 1; i >= 0; i--)
        {
            Material mat = _highlightLerpMaterials[i];
            if (mat == null ||
                !mat.HasProperty(emissionStrengthProperty) ||
                !_highlightLerpTargets.ContainsKey(mat))
            {
                _highlightLerpTargets.Remove(mat);
                _highlightLerpStarts.Remove(mat);
                _highlightLerpDurations.Remove(mat);
                _highlightLerpElapsed.Remove(mat);
                _highlightLerpHolds.Remove(mat);
                _highlightLerpMaterials.RemoveAt(i);
                continue;
            }

            if (_highlightLerpHolds.ContainsKey(mat) && _highlightLerpHolds[mat] > 0f)
            {
                _highlightLerpHolds[mat] -= Time.deltaTime;
                continue;
            }

            if (!_highlightLerpStarts.ContainsKey(mat) ||
                !_highlightLerpDurations.ContainsKey(mat) ||
                !_highlightLerpElapsed.ContainsKey(mat))
            {
                float immediateTarget = _highlightLerpTargets[mat];
                mat.SetFloat(emissionStrengthProperty, immediateTarget);
                _highlightLerpTargets.Remove(mat);
                _highlightLerpStarts.Remove(mat);
                _highlightLerpDurations.Remove(mat);
                _highlightLerpElapsed.Remove(mat);
                _highlightLerpHolds.Remove(mat);
                _highlightLerpMaterials.RemoveAt(i);
                continue;
            }

            float target = _highlightLerpTargets[mat];
            float start = _highlightLerpStarts[mat];
            float duration = Mathf.Max(0.0001f, _highlightLerpDurations[mat]);
            float elapsed = _highlightLerpElapsed[mat] + Time.deltaTime;
            _highlightLerpElapsed[mat] = elapsed;

            float t = Mathf.Clamp01(elapsed / duration);
            float next = Mathf.Lerp(start, target, t);
            mat.SetFloat(emissionStrengthProperty, next);

            if (t >= 1f || Mathf.Approximately(next, target))
            {
                _highlightLerpTargets.Remove(mat);
                _highlightLerpStarts.Remove(mat);
                _highlightLerpDurations.Remove(mat);
                _highlightLerpElapsed.Remove(mat);
                _highlightLerpHolds.Remove(mat);
                _highlightLerpMaterials.RemoveAt(i);
            }
        }
    }

    private void QueueHighlightLerp(
        Material mat,
        float target,
        float duration,
        bool resetToIdleBeforeLerp,
        float holdBeforeLerp)
    {
        if (mat == null || !mat.HasProperty(emissionStrengthProperty))
        {
            return;
        }

        if (resetToIdleBeforeLerp)
        {
            mat.SetFloat(emissionStrengthProperty, idleEmissionStrength);
        }

        float current = mat.GetFloat(emissionStrengthProperty);
        if (Mathf.Approximately(current, target) || duration <= 0f)
        {
            RemoveHighlightLerp(mat);
            mat.SetFloat(emissionStrengthProperty, target);
            return;
        }

        if (!_highlightLerpTargets.ContainsKey(mat))
        {
            _highlightLerpMaterials.Add(mat);
        }

        _highlightLerpTargets[mat] = target;
        _highlightLerpStarts[mat] = current;
        _highlightLerpDurations[mat] = Mathf.Max(0.0001f, duration);
        _highlightLerpElapsed[mat] = 0f;
        _highlightLerpHolds[mat] = Mathf.Max(0f, holdBeforeLerp);
    }

    private void RemoveHighlightLerp(Material mat)
    {
        if (mat == null)
        {
            return;
        }

        _highlightLerpTargets.Remove(mat);
        _highlightLerpStarts.Remove(mat);
        _highlightLerpDurations.Remove(mat);
        _highlightLerpElapsed.Remove(mat);
        _highlightLerpHolds.Remove(mat);
        _highlightLerpMaterials.Remove(mat);
    }

    private void ClearHighlightLerp()
    {
        _highlightLerpMaterials.Clear();
        _highlightLerpTargets.Clear();
        _highlightLerpStarts.Clear();
        _highlightLerpDurations.Clear();
        _highlightLerpElapsed.Clear();
        _highlightLerpHolds.Clear();
    }

    private bool ContainsExpectedItem(
        Dictionary<AvatarAttachmentPoint.AttachmentType, EquippableItem> equipped,
        EquippableItem expected)
    {
        foreach (EquippableItem equippedItem in equipped.Values)
        {
            if (IsMatch(equippedItem, expected))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsMatch(EquippableItem actual, EquippableItem expected)
    {
        if (actual == null || expected == null)
        {
            return false;
        }

        if (orderedMatchMode == OrderedMatchMode.ExactReference)
        {
            return actual == expected;
        }

        return actual.itemType == expected.itemType;
    }

    private static bool IsAttachmentTypeCompatible(
        AvatarAttachmentPoint.AttachmentType itemType,
        AvatarAttachmentPoint.AttachmentType slotType)
    {
        if (itemType == slotType)
        {
            return true;
        }

        if (itemType == AvatarAttachmentPoint.AttachmentType.Belt &&
            (slotType == AvatarAttachmentPoint.AttachmentType.BeltLeft ||
             slotType == AvatarAttachmentPoint.AttachmentType.BeltRight))
        {
            return true;
        }

        if (itemType == AvatarAttachmentPoint.AttachmentType.BeltLeft &&
            slotType == AvatarAttachmentPoint.AttachmentType.Belt)
        {
            return true;
        }

        if (itemType == AvatarAttachmentPoint.AttachmentType.BeltRight &&
            slotType == AvatarAttachmentPoint.AttachmentType.Belt)
        {
            return true;
        }

        if (itemType == AvatarAttachmentPoint.AttachmentType.Chest &&
            slotType == AvatarAttachmentPoint.AttachmentType.ChestLeft)
        {
            return true;
        }

        if (itemType == AvatarAttachmentPoint.AttachmentType.ChestLeft &&
            slotType == AvatarAttachmentPoint.AttachmentType.Chest)
        {
            return true;
        }

        return false;
    }

    private static void SetItemLocked(EquippableItem item, bool locked)
    {
        if (item == null)
        {
            return;
        }

        HandGrabInteractable handGrabInteractable = item.GetComponent<HandGrabInteractable>();
        if (handGrabInteractable != null)
        {
            handGrabInteractable.enabled = !locked;
        }
    }

    private static bool ShouldLockItem(EquippableItem item)
    {
        return item != null && item.itemType != EquippableItem.ItemType.Baton;
    }
}
