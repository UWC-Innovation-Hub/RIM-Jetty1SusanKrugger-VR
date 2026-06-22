using System;
using System.Reflection;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CellHoldSelector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PrisonerSortModule prisonerSortModule;
    [SerializeField] private InteractableUnityEventWrapper cellInteractable;
    [SerializeField] private GameObject cellRouteObject;
    [SerializeField] private Material cellRouteMaterial;

    [Header("Interaction")]
    [Min(0.1f)]
    [SerializeField] private float holdDuration = 3f;

    [Header("Indicator")]
    [SerializeField] private Sprite progressSprite;
    [SerializeField] private Vector2 indicatorSize = new Vector2(100f, 100f);
    [SerializeField] private Vector3 indicatorScale = new Vector3(0.0004f, 0.0004f, 0.0004f);
    [SerializeField] private Transform leftHandAnchorOverride;
    [SerializeField] private Vector3 leftHandLocalOffset = new Vector3(0f, 0.08f, 0.02f);
    [SerializeField] private Vector3 leftHandLocalEulerAngles = new Vector3(-60f, 180f, 0f);
    [SerializeField] private Color backgroundColor = new Color(0.8962264f, 0.8962264f, 0.8962264f, 0.2f);
    [SerializeField] private Color fillColor = new Color(1f, 0.925f, 0.6f, 1f);
    [SerializeField] private Color selectedFillColor = new Color(1f, 0.45f, 0.35f, 1f);

    [Header("Cell Highlight")]
    [SerializeField] private Color idleEmissionColor = new Color(1f, 0.8509f, 0.2980f);
    [SerializeField] private float inactiveEmissionStrength = 0f;
    [SerializeField] private float idleEmissionStrength = 0f;
    [SerializeField] private float hoverEmissionStrength = 0f;
    [SerializeField] private Color selectedEmissionColor = new Color(0.3537736f, 0.49710897f, 1f, 1f);
    [SerializeField] private float selectedEmissionStrength = 0f;

    private UnityAction _hoverListener;
    private UnityAction _unhoverListener;
    private bool _listenersBound;
    private bool _interactionEnabled = true;
    private bool _selectionLocked;
    private bool _isHolding;
    private float _holdElapsed;
    private Transform _leftHandAnchor;
    private RectTransform _indicatorRoot;
    private CanvasGroup _indicatorCanvasGroup;
    private Image _fillArc;

    public bool IsSelectionCommitted => _selectionLocked;

    private void Reset()
    {
        ResolveDependencies();
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
        HideIndicator();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Update()
    {
        if (!_interactionEnabled || _selectionLocked || !_isHolding)
            return;

        _holdElapsed += Time.deltaTime;
        float normalized = Mathf.Clamp01(_holdElapsed / holdDuration);
        ShowIndicator(normalized, fillColor);
        ApplyCellMaterial(hoverEmissionStrength, idleEmissionColor);

        if (normalized >= 1f)
            CommitSelection();
    }

    public void Bind(PrisonerSortModule module)
    {
        prisonerSortModule = module;
    }

    public void ResetSelectionState()
    {
        ResolveDependencies();
        EnsureHandIndicator();
        _interactionEnabled = true;
        _selectionLocked = false;
        _isHolding = false;
        _holdElapsed = 0f;
        ApplyCellMaterial(ResolveIdleEmissionStrength(), idleEmissionColor);
        HideIndicator();
        SetWrapperEnabled(true);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        _interactionEnabled = enabled;

        if (!enabled)
        {
            _isHolding = false;
            _holdElapsed = 0f;
            HideIndicator();

            if (!_selectionLocked)
                ApplyCellMaterial(inactiveEmissionStrength, idleEmissionColor);
        }

        SetWrapperEnabled(enabled);
    }

    public void SelectCellNow()
    {
        if (!_interactionEnabled || _selectionLocked)
            return;

        CommitSelection();
    }

    private void Initialize()
    {
        ResolveDependencies();
        EnsureHandIndicator();
        ApplyCellMaterial(ResolveIdleEmissionStrength(), idleEmissionColor);
        HideIndicator();
    }

    private void ResolveDependencies()
    {
        if (prisonerSortModule == null)
            prisonerSortModule = GetComponentInParent<PrisonerSortModule>();

        if (prisonerSortModule == null)
            prisonerSortModule = FindFirstObjectByType<PrisonerSortModule>();

        if (cellInteractable == null)
            cellInteractable = GetComponentInChildren<InteractableUnityEventWrapper>(true);

        if (cellRouteObject == null && cellInteractable != null)
            cellRouteObject = cellInteractable.gameObject;
    }

    private void BindEvents()
    {
        if (cellInteractable == null || _listenersBound)
            return;

        _hoverListener = HandleHover;
        _unhoverListener = HandleUnhover;

        bool hoverBound = TrySetWrapperListener(cellInteractable, "WhenHover", _hoverListener, add: true);
        bool unhoverBound = TrySetWrapperListener(cellInteractable, "WhenUnhover", _unhoverListener, add: true);
        _listenersBound = hoverBound || unhoverBound;
    }

    private void UnbindEvents()
    {
        if (cellInteractable == null || !_listenersBound)
            return;

        if (_hoverListener != null)
            TrySetWrapperListener(cellInteractable, "WhenHover", _hoverListener, add: false);

        if (_unhoverListener != null)
            TrySetWrapperListener(cellInteractable, "WhenUnhover", _unhoverListener, add: false);

        _listenersBound = false;
    }

    private void HandleHover()
    {
        if (!_interactionEnabled || _selectionLocked)
            return;

        _isHolding = true;
        _holdElapsed = 0f;
        ShowIndicator(0f, fillColor);
        ApplyCellMaterial(hoverEmissionStrength, idleEmissionColor);
    }

    private void HandleUnhover()
    {
        if (!_interactionEnabled || _selectionLocked)
            return;

        _isHolding = false;
        _holdElapsed = 0f;
        HideIndicator();
        ApplyCellMaterial(ResolveIdleEmissionStrength(), idleEmissionColor);
    }

    private void CommitSelection()
    {
        _selectionLocked = true;
        _interactionEnabled = false;
        _isHolding = false;
        _holdElapsed = holdDuration;

        ShowIndicator(1f, selectedFillColor);
        ApplyCellMaterial(selectedEmissionStrength, selectedEmissionColor);
        prisonerSortModule?.SendBatchToCell();
        HideIndicator();
        SetWrapperEnabled(false);
    }

    private void SetWrapperEnabled(bool enabled)
    {
        if (cellInteractable != null)
            cellInteractable.enabled = enabled;
    }

    private void ApplyCellMaterial(float emissionStrength, Color emissionColor)
    {
        if (cellRouteMaterial == null)
            return;

        cellRouteMaterial.SetColor("_EmissionColor", emissionColor);
        cellRouteMaterial.SetFloat("_EmissionStrength", emissionStrength);
    }

    private float ResolveIdleEmissionStrength()
    {
        bool moduleActive = prisonerSortModule != null && prisonerSortModule.IsActive;
        return moduleActive ? idleEmissionStrength : inactiveEmissionStrength;
    }

    private void EnsureHandIndicator()
    {
        if (progressSprite == null)
            return;

        _leftHandAnchor = ResolveLeftHandAnchor();
        if (_leftHandAnchor == null)
            return;

        if (_indicatorRoot == null)
        {
            Transform existing = _leftHandAnchor.Find("LeftHandCellHoldIndicator");
            if (existing == null)
                existing = CreateIndicatorRoot(_leftHandAnchor, _leftHandAnchor.gameObject.layer).transform;

            _indicatorRoot = existing as RectTransform;
            _indicatorCanvasGroup = existing.GetComponent<CanvasGroup>();
            _fillArc = existing.Find("FillArc")?.GetComponent<Image>();
        }

        if (_indicatorRoot.parent != _leftHandAnchor)
            _indicatorRoot.SetParent(_leftHandAnchor, false);

        _indicatorRoot.localPosition = leftHandLocalOffset;
        _indicatorRoot.localRotation = Quaternion.Euler(leftHandLocalEulerAngles);
        _indicatorRoot.localScale = indicatorScale;

        if (_fillArc != null)
        {
            _fillArc.sprite = progressSprite;
            _fillArc.color = fillColor;
            _fillArc.type = Image.Type.Filled;
            _fillArc.fillMethod = Image.FillMethod.Radial360;
            _fillArc.fillOrigin = 0;
            _fillArc.fillClockwise = true;
            _fillArc.fillAmount = 0f;
            _fillArc.raycastTarget = false;
        }

        SetIndicatorVisible(false);
    }

    private GameObject CreateIndicatorRoot(Transform parent, int layer)
    {
        GameObject indicatorRoot = new GameObject("LeftHandCellHoldIndicator", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        indicatorRoot.layer = layer;
        indicatorRoot.transform.SetParent(parent, false);

        RectTransform rootRect = indicatorRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = indicatorSize;
        rootRect.localScale = indicatorScale;
        rootRect.localPosition = Vector3.zero;
        rootRect.localRotation = Quaternion.identity;

        Canvas canvas = indicatorRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = indicatorRoot.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1f;
        scaler.referencePixelsPerUnit = 100f;

        CreateImageChild(indicatorRoot.transform, "BackgroundArc", backgroundColor, false);
        CreateImageChild(indicatorRoot.transform, "FillArc", fillColor, true);

        return indicatorRoot;
    }

    private void CreateImageChild(Transform parent, string childName, Color color, bool filled)
    {
        GameObject imageObject = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.sizeDelta = indicatorSize;
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.sprite = progressSprite;
        image.raycastTarget = false;

        if (!filled)
            return;

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 0;
        image.fillClockwise = true;
        image.fillAmount = 0f;
    }

    private Transform ResolveLeftHandAnchor()
    {
        if (leftHandAnchorOverride != null)
            return leftHandAnchorOverride;

        Transform anchor = FindTransformByName("LeftHandAnchor");
        if (anchor != null)
            return anchor;

        return FindTransformByName("LeftControllerInHandAnchor");
    }

    private Transform FindTransformByName(string targetName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == targetName)
                return transforms[i];
        }

        return null;
    }

    private void ShowIndicator(float fillAmount, Color fillArcColor)
    {
        EnsureHandIndicator();

        if (_fillArc == null)
            return;

        SetIndicatorVisible(true);
        _fillArc.color = fillArcColor;
        _fillArc.fillAmount = Mathf.Clamp01(fillAmount);
    }

    private void HideIndicator()
    {
        if (_fillArc != null)
        {
            _fillArc.fillAmount = 0f;
            _fillArc.color = fillColor;
        }

        SetIndicatorVisible(false);
    }

    private void SetIndicatorVisible(bool visible)
    {
        if (_indicatorCanvasGroup == null)
            return;

        _indicatorCanvasGroup.alpha = visible ? 1f : 0f;
        _indicatorCanvasGroup.interactable = false;
        _indicatorCanvasGroup.blocksRaycasts = false;
    }

    private static bool TrySetWrapperListener(InteractableUnityEventWrapper wrapper, string eventName, UnityAction action, bool add)
    {
        if (wrapper == null || action == null)
            return false;

        UnityEvent unityEvent = ResolveWrapperEvent(wrapper, eventName);
        if (unityEvent == null)
            return false;

        if (add)
            unityEvent.AddListener(action);
        else
            unityEvent.RemoveListener(action);

        return true;
    }

    private static UnityEvent ResolveWrapperEvent(InteractableUnityEventWrapper wrapper, string eventName)
    {
        Type wrapperType = wrapper.GetType();
        PropertyInfo property = wrapperType.GetProperty(eventName, BindingFlags.Instance | BindingFlags.Public);
        if (property?.GetValue(wrapper) is UnityEvent propertyEvent)
            return propertyEvent;

        FieldInfo directField = wrapperType.GetField(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (directField?.GetValue(wrapper) is UnityEvent directEvent)
            return directEvent;

        string backingFieldName = $"_{char.ToLowerInvariant(eventName[0])}{eventName.Substring(1)}";
        FieldInfo backingField = wrapperType.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField?.GetValue(wrapper) is UnityEvent backingEvent)
            return backingEvent;

        return null;
    }
}
