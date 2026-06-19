using Oculus.Interaction.HandGrab;
using System.Collections;
using System.Collections.Generic;
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

    private class HighlightLerpState
    {
        public float start;
        public float target;
        public float duration;
        public float elapsed;
        public float hold;
    }

    [Header("Wiring")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Completion Rule")]
    [SerializeField] private int requiredEquippedCount = 4;

    [Header("Initial Gaze Gate")]
    [SerializeField] private bool requireInitialGazeGate = false;
    [SerializeField] private GazeTarget initialGazeTarget;
    [SerializeField] private AudioSource initialGazeAudioSource;
    [SerializeField] private GameObject initialGazeUiRoot;
    [SerializeField] private CanvasGroup initialGazeUiCanvasGroup;
    [SerializeField] private float initialGazeUiFadeDuration = 0.35f;
    [SerializeField] private bool lockOrderedItemsUntilGazeAudioFinished = true;

    [Header("Order Enforcement")]
    [SerializeField] private AudioSource InventoryAudio;
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

    [Header("Relocate Settings")]
    public float relocateDelay = 1f;
    public PlayerRelocate playerRelocate;

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
    private bool _initialGazeSubscribed;
    private bool _initialGazeGateOpen = true;
    private bool _initialGazeAccepted;
    private Coroutine _initialGazeRoutine;
    private readonly Dictionary<Material, HighlightLerpState> _highlightLerps = new Dictionary<Material, HighlightLerpState>();
    private readonly List<Material> _completedHighlightLerps = new List<Material>();
    private readonly Dictionary<EquippableItem, Material[]> _highlightMaterialsByItem = new Dictionary<EquippableItem, Material[]>();
    private readonly Dictionary<EquippableItem, AvatarAttachmentPoint[]> _attachmentPointsByItem = new Dictionary<EquippableItem, AvatarAttachmentPoint[]>();
    private readonly Dictionary<EquippableItem.ItemType, AvatarAttachmentPoint[]> _attachmentPointsByItemType = new Dictionary<EquippableItem.ItemType, AvatarAttachmentPoint[]>();

    private void Update()
    {
        TickHighlightLerpIn();
    }

    public override void Activate()
    {
        base.Activate();

        ResolveInventoryManager();

        ResetProgress();

        ResetInitialGazeGate();
        SubscribeInventoryEvents();
        SubscribeInitialGazeEvents();
        ResolveAttachmentPoints();
        BuildBindingCaches();
        SetAllConfiguredHighlights(idleEmissionStrength);
        RebuildProgressFromInventory();
        RefreshInventoryFeedback();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        UnsubscribeInventoryEvents();
        UnsubscribeInitialGazeEvents();
        StopInitialGazeRoutine();
        if (IsInitialGazeGateClosed())
        {
            SetOrderedItemsLocked(false);
        }
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

        if (IsInitialGazeGateClosed())
        {
            RejectEquippedItem(slot);
            return;
        }

        if (!enforceOrder)
        {
            if (_acceptedItems.Add(item))
            {
                CorrectPlacedCount = _acceptedItems.Count;
                CurrentStepIndex = CorrectPlacedCount;
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

        if (lockCorrectlyPlacedItems)
        {
            SetItemLocked(item, true);
        }

        RefreshInventoryFeedback();
    }

    private void OnItemUnequipped(AvatarAttachmentPoint.AttachmentType _)
    {
        if (!IsActive || IsComplete)
        {
            return;
        }

        RebuildProgressFromInventory();
        RefreshInventoryFeedback();
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

    private void ResetProgress()
    {
        _acceptedItems.Clear();
        CorrectPlacedCount = 0;
        CurrentStepIndex = 0;
    }

    private void RefreshInventoryFeedback()
    {
        UpdateHighlight();
        UpdateAttachmentPointHighlights();
        TryCompleteIfReady();
    }

    private void BuildBindingCaches()
    {
        _highlightMaterialsByItem.Clear();
        _attachmentPointsByItem.Clear();
        _attachmentPointsByItemType.Clear();

        if (highlightBindings != null)
        {
            for (int i = 0; i < highlightBindings.Count; i++)
            {
                ItemHighlightBinding binding = highlightBindings[i];
                if (binding?.item != null)
                {
                    _highlightMaterialsByItem[binding.item] = binding.highlightMaterials;
                }
            }
        }

        if (itemAttachmentBindings == null)
        {
            return;
        }

        for (int i = 0; i < itemAttachmentBindings.Count; i++)
        {
            ItemAttachmentBinding binding = itemAttachmentBindings[i];
            if (binding?.item != null)
            {
                _attachmentPointsByItem[binding.item] = binding.targetAttachmentPoints;
                _attachmentPointsByItemType[binding.item.itemType] = binding.targetAttachmentPoints;
            }
        }
    }

    private void ResolveInitialGazeCanvasGroup()
    {
        if (initialGazeUiCanvasGroup != null)
        {
            return;
        }

        if (initialGazeUiRoot == null)
        {
            return;
        }

        initialGazeUiCanvasGroup = initialGazeUiRoot.GetComponent<CanvasGroup>();
        if (initialGazeUiCanvasGroup == null)
        {
            initialGazeUiCanvasGroup = initialGazeUiRoot.AddComponent<CanvasGroup>();
        }
    }

    private void ResetInitialGazeGate()
    {
        StopInitialGazeRoutine();

        _initialGazeAccepted = false;
        _initialGazeGateOpen = !requireInitialGazeGate;

        if (!requireInitialGazeGate)
        {
            return;
        }

        ResolveInitialGazeCanvasGroup();
        if (initialGazeTarget == null)
        {
            Debug.LogWarning($"{name}: Initial gaze gate is enabled but no GazeTarget is assigned.");
            _initialGazeGateOpen = true;
            return;
        }

        if (initialGazeAudioSource == null)
        {
            Debug.LogWarning($"{name}: Initial gaze gate has no AudioSource assigned. It will open immediately after gaze dwell.");
        }

        if (initialGazeUiRoot == null && initialGazeUiCanvasGroup == null)
        {
            Debug.LogWarning($"{name}: Initial gaze gate has no UI root or CanvasGroup assigned.");
        }

        if (initialGazeUiRoot != null)
        {
            initialGazeUiRoot.SetActive(true);
        }

        SetInitialGazeUiState(visible: true);

        if (lockOrderedItemsUntilGazeAudioFinished)
        {
            SetOrderedItemsLocked(true);
        }
    }

    private void SubscribeInitialGazeEvents()
    {
        if (!requireInitialGazeGate || _initialGazeSubscribed || initialGazeTarget == null)
        {
            return;
        }

        initialGazeTarget.onGazeDwell.AddListener(OnInitialGazeDwell);
        _initialGazeSubscribed = true;
    }

    private void UnsubscribeInitialGazeEvents()
    {
        if (!_initialGazeSubscribed || initialGazeTarget == null)
        {
            _initialGazeSubscribed = false;
            return;
        }

        initialGazeTarget.onGazeDwell.RemoveListener(OnInitialGazeDwell);
        _initialGazeSubscribed = false;
    }

    private void OnInitialGazeDwell()
    {
        if (!IsActive || IsComplete || _initialGazeGateOpen || _initialGazeAccepted)
        {
            return;
        }

        _initialGazeAccepted = true;

        StopInitialGazeRoutine();
        _initialGazeRoutine = StartCoroutine(RunInitialGazeGateRoutine());
    }

    private IEnumerator RunInitialGazeGateRoutine()
    {
        ResolveInitialGazeCanvasGroup();

        if (initialGazeUiRoot != null && !initialGazeUiRoot.activeSelf)
        {
            initialGazeUiRoot.SetActive(true);
        }

        float fadeDuration = Mathf.Max(0f, initialGazeUiFadeDuration);
        float fadeElapsed = 0f;
        float startAlpha = initialGazeUiCanvasGroup != null ? initialGazeUiCanvasGroup.alpha : 0f;

        yield return null;

        while (IsActive &&
               !IsComplete &&
               ((initialGazeAudioSource != null && initialGazeAudioSource.isPlaying) ||
                fadeElapsed < fadeDuration))
        {
            if (initialGazeUiCanvasGroup != null && fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(fadeElapsed / fadeDuration);
                initialGazeUiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }

            initialGazeUiCanvasGroup.gameObject.SetActive(false);

            yield return null;
        }

        HideInitialGazeUi();
        _initialGazeRoutine = null;

        if (!IsActive || IsComplete)
        {
            yield break;
        }

        OpenInitialGazeGate();
    }

    private void OpenInitialGazeGate()
    {
        if (_initialGazeGateOpen)
        {
            return;
        }

        _initialGazeGateOpen = true;

        if (lockOrderedItemsUntilGazeAudioFinished)
        {
            SetOrderedItemsLocked(false);
        }

        InventoryAudio.Play();

        RebuildProgressFromInventory();
        RefreshInventoryFeedback();
    }

    private void HideInitialGazeUi()
    {
        SetInitialGazeUiState(visible: false);

        if (initialGazeUiRoot != null)
        {
            initialGazeUiRoot.SetActive(false);
        }
    }

    private void SetInitialGazeUiState(bool visible)
    {
        if (initialGazeUiCanvasGroup != null)
        {
            initialGazeUiCanvasGroup.alpha = visible ? 1f : 0f;
            initialGazeUiCanvasGroup.interactable = visible;
            initialGazeUiCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void StopInitialGazeRoutine()
    {
        if (_initialGazeRoutine != null)
        {
            StopCoroutine(_initialGazeRoutine);
            _initialGazeRoutine = null;
        }
    }

    private bool IsInitialGazeGateClosed()
    {
        return requireInitialGazeGate && !_initialGazeGateOpen;
    }

    private void SetOrderedItemsLocked(bool locked)
    {
        if (orderedItems == null)
        {
            return;
        }

        for (int i = 0; i < orderedItems.Count; i++)
        {
            SetItemLocked(orderedItems[i], locked);
        }
    }

    private void RejectEquippedItem(AvatarAttachmentPoint.AttachmentType slot)
    {
        ResetProgress();

        if (inventoryManager != null)
        {
            inventoryManager.UnequipItem(slot);
        }
    }

    private void RebuildProgressFromInventory()
    {
        ResetProgress();

        if (inventoryManager == null || IsInitialGazeGateClosed())
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
    }

    private int GetTargetCount()
    {
        int target = Mathf.Max(1, requiredEquippedCount);
        return enforceOrder && orderedItems != null && orderedItems.Count > 0
            ? Mathf.Min(target, orderedItems.Count)
            : target;
    }

    private EquippableItem GetExpectedItem(int stepIndex) =>
        orderedItems != null && stepIndex >= 0 && stepIndex < orderedItems.Count
            ? orderedItems[stepIndex]
            : null;

    private void TryCompleteIfReady()
    {
        if (IsInitialGazeGateClosed())
        {
            return;
        }

        if (CorrectPlacedCount >= GetTargetCount())
        {
            Complete();

            StartCoroutine(FadeOutAudioSource(
       InventoryAudio,
       1f
   ));

            StartCoroutine(RelocateAfterDelay());
        }
    }


    private IEnumerator FadeOutAudioSource(AudioSource audioSource, float duration)
    {
        if (audioSource == null)
        {
            yield break;
        }

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private IEnumerator RelocateAfterDelay()
    {

        yield return new WaitForSeconds(relocateDelay);

        playerRelocate.Relocate();
    }

    private void UpdateHighlight()
    {
        if (!useHighlight || !enforceOrder || IsInitialGazeGateClosed())
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

        if (!IsActive || !enforceOrder || IsInitialGazeGateClosed())
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

        return AvatarAttachmentPoint.IsCompatible(expectedItem.compatibleAttachmentPoint, point.attachmentType);
    }

    private AvatarAttachmentPoint[] GetExplicitAttachmentPoints(EquippableItem expectedItem)
    {
        if (expectedItem == null)
        {
            return null;
        }

        if (_attachmentPointsByItem.TryGetValue(expectedItem, out AvatarAttachmentPoint[] points))
        {
            return points;
        }

        return orderedMatchMode == OrderedMatchMode.ItemType &&
               _attachmentPointsByItemType.TryGetValue(expectedItem.itemType, out points)
            ? points
            : null;
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
        Material[] materials = GetHighlightMaterials(item);
        if (!useHighlight || materials == null || materials.Length == 0)
        {
            return;
        }

        float targetStrength = highlighted ? highlightedEmissionStrength : idleEmissionStrength;
        bool useLerp = highlighted ? lerpHighlightIn : lerpHighlightOut;
        float duration = highlighted ? highlightLerpDuration : highlightOutLerpDuration;
        float hold = highlighted ? highlightLerpInHoldDuration : 0f;

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null || !mat.HasProperty(emissionStrengthProperty))
            {
                continue;
            }

            if (useLerp)
            {
                QueueHighlightLerp(mat, targetStrength, duration, highlighted, hold);
                continue;
            }

            RemoveHighlightLerp(mat);
            mat.SetFloat(emissionStrengthProperty, targetStrength);
        }
    }

    private Material[] GetHighlightMaterials(EquippableItem item)
    {
        return item != null && _highlightMaterialsByItem.TryGetValue(item, out Material[] materials)
            ? materials
            : null;
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
        if (_highlightLerps.Count == 0)
        {
            return;
        }

        _completedHighlightLerps.Clear();
        foreach (KeyValuePair<Material, HighlightLerpState> entry in _highlightLerps)
        {
            Material mat = entry.Key;
            HighlightLerpState state = entry.Value;
            if (mat == null || state == null || !mat.HasProperty(emissionStrengthProperty))
            {
                _completedHighlightLerps.Add(mat);
                continue;
            }

            if (state.hold > 0f)
            {
                state.hold -= Time.deltaTime;
                continue;
            }

            state.elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(state.elapsed / Mathf.Max(0.0001f, state.duration));
            float next = Mathf.Lerp(state.start, state.target, t);
            mat.SetFloat(emissionStrengthProperty, next);

            if (t >= 1f || Mathf.Approximately(next, state.target))
            {
                _completedHighlightLerps.Add(mat);
            }
        }

        for (int i = 0; i < _completedHighlightLerps.Count; i++)
        {
            _highlightLerps.Remove(_completedHighlightLerps[i]);
        }

        _completedHighlightLerps.Clear();
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

        _highlightLerps[mat] = new HighlightLerpState
        {
            start = current,
            target = target,
            duration = Mathf.Max(0.0001f, duration),
            elapsed = 0f,
            hold = Mathf.Max(0f, holdBeforeLerp)
        };
    }

    private void RemoveHighlightLerp(Material mat)
    {
        _highlightLerps.Remove(mat);
    }

    private void ClearHighlightLerp()
    {
        _highlightLerps.Clear();
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

        return orderedMatchMode == OrderedMatchMode.ExactReference
            ? actual == expected
            : actual.itemType == expected.itemType;
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

}
