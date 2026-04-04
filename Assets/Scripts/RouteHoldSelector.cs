using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Oculus.Interaction;

[DisallowMultipleComponent]
public class RouteHoldSelector : MonoBehaviour
{
    [Serializable]
    private class RouteConfig
    {
        public string routeId;
        public GameObject routeObject;
        public InteractableUnityEventWrapper interactable;
        public Material routeMaterial;
        [NonSerialized] public UnityAction hoverListener;
        [NonSerialized] public UnityAction unhoverListener;
        [NonSerialized] public bool listenersBound;
    }

    private static readonly string[] RouteOrder = { "cell", "truck", "boat" };

    [Header("Dependencies")]
    [SerializeField] private PrisonerRoute prisonerRoute;
    [SerializeField] private PrisonerSortModule prisonerSortModule;
    [SerializeField] private ReactivateRoutes routeController;

    [Header("Interaction")]
    [Min(0.1f)]
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private bool autoDiscoverRoutes = true;

    [Header("Indicator")]
    [SerializeField] private Sprite progressSprite;
    [SerializeField] private Vector2 indicatorSize = new Vector2(124.78f, 54.11f);
    [SerializeField] private Vector3 indicatorScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
    [SerializeField] private Transform leftHandAnchorOverride;
    [SerializeField] private Vector3 leftHandLocalOffset = new Vector3(0f, 0.08f, 0.02f);
    [SerializeField] private Vector3 leftHandLocalEulerAngles = new Vector3(0f, 180f, 0f);
    [SerializeField] private float indicatorHeightPadding = 0.2f;
    [SerializeField] private Color backgroundColor = new Color(1f, 0.8509f, 0.2980f, 0.2f);
    [SerializeField] private Color fillColor = new Color(1f, 0.925f, 0.6f, 1f);
    [SerializeField] private Color selectedFillColor = new Color(1f, 0.45f, 0.35f, 1f);

    [Header("Route Highlight")]
    [SerializeField] private Color idleEmissionColor = new Color(1f, 0.8509f, 0.2980f);
    [SerializeField] private float idleEmissionStrength = 0.1f;
    [SerializeField] private float hoverEmissionStrength = 1f;
    [SerializeField] private Color selectedEmissionColor = Color.red;
    [SerializeField] private float selectedEmissionStrength = 1f;

    [Header("Auto-Discovered Routes")]
    [SerializeField] private RouteConfig[] routes = new RouteConfig[RouteOrder.Length];

    private int _activeRouteIndex = -1;
    private float _holdElapsed;
    private bool _selectionLocked;
    private bool _interactionEnabled = true;
    private Transform _leftHandAnchor;
    private RectTransform _indicatorRoot;
    private CanvasGroup _indicatorCanvasGroup;
    private Image _backgroundArc;
    private Image _fillArc;

    public bool IsSelectionCommitted => _selectionLocked;

    private void Reset()
    {
        ResolveDependencies();
        DiscoverRoutes();
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
        if (!_interactionEnabled || _selectionLocked || _activeRouteIndex < 0 || _activeRouteIndex >= routes.Length)
            return;

        RouteConfig route = routes[_activeRouteIndex];
        if (route == null)
            return;

        _holdElapsed += Time.deltaTime;
        float normalized = Mathf.Clamp01(_holdElapsed / holdDuration);
        ShowIndicator(normalized, fillColor);
        ApplyHoverMaterial(_activeRouteIndex);

        if (normalized >= 1f)
            CommitSelection(_activeRouteIndex);
    }

    public void ResetSelectionState()
    {
        ResolveDependencies();

        if (autoDiscoverRoutes || !AreRoutesConfigured())
            DiscoverRoutes();

        EnsureHandIndicator();
        _interactionEnabled = true;
        _selectionLocked = false;
        _holdElapsed = 0f;
        _activeRouteIndex = -1;
        ResetRouteMaterials();
        HideIndicator();
        SetWrapperEnabled(true);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        _interactionEnabled = enabled;
        _holdElapsed = 0f;
        _activeRouteIndex = enabled ? _activeRouteIndex : -1;

        if (!enabled && !_selectionLocked)
            ResetRouteMaterials();

        if (!enabled)
            HideIndicator();

        SetWrapperEnabled(enabled);
    }

    private void Initialize()
    {
        ResolveDependencies();

        if (autoDiscoverRoutes || !AreRoutesConfigured())
            DiscoverRoutes();

        EnsureHandIndicator();
        BindSelectorToPrisonerRoute();
        ResetRouteMaterials();
        HideIndicator();
    }

    private void ResolveDependencies()
    {
        if (!prisonerRoute)
            prisonerRoute = GetComponent<PrisonerRoute>();

        if (!prisonerRoute)
            prisonerRoute = GetComponentInChildren<PrisonerRoute>(true);

        if (!prisonerRoute)
            prisonerRoute = FindFirstObjectByType<PrisonerRoute>();

        if (!prisonerSortModule)
            prisonerSortModule = GetComponentInParent<PrisonerSortModule>();

        if (!prisonerSortModule)
            prisonerSortModule = FindFirstObjectByType<PrisonerSortModule>();

        if (!routeController)
            routeController = GetComponentInParent<ReactivateRoutes>();

        if (!routeController)
            routeController = FindFirstObjectByType<ReactivateRoutes>();
    }

    private void BindSelectorToPrisonerRoute()
    {
        prisonerRoute?.BindHoldSelector(this);
    }

    private bool AreRoutesConfigured()
    {
        if (routes == null || routes.Length != RouteOrder.Length)
            return false;

        for (int i = 0; i < routes.Length; i++)
        {
            if (routes[i] == null || routes[i].interactable == null || routes[i].routeObject == null || string.IsNullOrWhiteSpace(routes[i].routeId))
                return false;
        }

        return true;
    }

    private void DiscoverRoutes()
    {
        RouteConfig[] discovered = new RouteConfig[RouteOrder.Length];

        for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
        {
            Transform child = transform.GetChild(childIndex);
            InteractableUnityEventWrapper interactable = child.GetComponentInChildren<InteractableUnityEventWrapper>(true);
            if (interactable == null)
                continue;

            string routeId = ResolveRouteId(interactable.gameObject.name, child.name);
            int routeIndex = Array.IndexOf(RouteOrder, routeId);
            if (routeIndex < 0)
                continue;

            RouteConfig route = discovered[routeIndex] ?? new RouteConfig();
            route.routeId = routeId;
            route.routeObject = interactable.gameObject;
            route.interactable = interactable;
            route.routeMaterial = ResolveRouteMaterial(interactable.gameObject, routeIndex);
            discovered[routeIndex] = route;
        }

        for (int routeIndex = 0; routeIndex < discovered.Length; routeIndex++)
        {
            if (discovered[routeIndex] == null)
            {
                discovered[routeIndex] = new RouteConfig
                {
                    routeId = RouteOrder[routeIndex]
                };
            }
        }

        routes = discovered;
    }

    private Material ResolveRouteMaterial(GameObject routeObject, int fallbackIndex)
    {
        if (routeController == null || routeController.Routes == null || routeController.RouteMats == null)
            return null;

        for (int i = 0; i < routeController.Routes.Length; i++)
        {
            if (routeController.Routes[i] == routeObject)
            {
                if (i >= 0 && i < routeController.RouteMats.Length)
                    return routeController.RouteMats[i];

                return null;
            }
        }

        if (fallbackIndex >= 0 && fallbackIndex < routeController.RouteMats.Length)
            return routeController.RouteMats[fallbackIndex];

        return null;
    }

    private void EnsureHandIndicator()
    {
        if (progressSprite == null)
            return;

        _leftHandAnchor = ResolveLeftHandAnchor();
        if (_leftHandAnchor == null)
        {
            Debug.LogWarning($"{name}: Could not resolve LeftHandAnchor or LeftControllerInHandAnchor for the hold indicator.");
            return;
        }

        if (_indicatorRoot == null)
        {
            Transform existing = _leftHandAnchor.Find("LeftHandRouteHoldIndicator");
            if (existing == null)
                existing = CreateIndicatorRoot(_leftHandAnchor, _leftHandAnchor.gameObject.layer, "LeftHandRouteHoldIndicator").transform;

            _indicatorRoot = existing as RectTransform;
            _indicatorCanvasGroup = existing.GetComponent<CanvasGroup>();
            _backgroundArc = existing.Find("BackgroundArc")?.GetComponent<Image>();
            _fillArc = existing.Find("FillArc")?.GetComponent<Image>();
        }

        if (_indicatorRoot.parent != _leftHandAnchor)
            _indicatorRoot.SetParent(_leftHandAnchor, false);

        _indicatorRoot.localPosition = leftHandLocalOffset;
        _indicatorRoot.localRotation = Quaternion.Euler(leftHandLocalEulerAngles);
        _indicatorRoot.localScale = indicatorScale;

        if (_backgroundArc != null)
        {
            _backgroundArc.sprite = progressSprite;
            _backgroundArc.color = backgroundColor;
            _backgroundArc.raycastTarget = false;
        }

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

    private GameObject CreateIndicatorRoot(Transform parent, int layer, string rootName)
    {
        GameObject indicatorRoot = new GameObject(rootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
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

        if (filled)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = 0;
            image.fillClockwise = true;
            image.fillAmount = 0f;
        }
    }

    private Transform ResolveLeftHandAnchor()
    {
        if (leftHandAnchorOverride != null)
            return leftHandAnchorOverride;

        Transform anchor = FindTransformByName("LeftHandAnchor");
        if (anchor != null)
            return anchor;

        anchor = FindTransformByName("LeftControllerInHandAnchor");
        if (anchor != null)
            return anchor;

        return null;
    }

    private void BindEvents()
    {
        for (int i = 0; i < routes.Length; i++)
        {
            RouteConfig route = routes[i];
            if (route == null || route.interactable == null || route.listenersBound)
                continue;

            int routeIndex = i;
            route.hoverListener = () => HandleRouteHover(routeIndex);
            route.unhoverListener = () => HandleRouteUnhover(routeIndex);

            bool hoverBound = TrySetWrapperListener(route.interactable, "WhenHover", route.hoverListener, add: true);
            bool unhoverBound = TrySetWrapperListener(route.interactable, "WhenUnhover", route.unhoverListener, add: true);
            route.listenersBound = hoverBound || unhoverBound;
        }
    }

    private void UnbindEvents()
    {
        if (routes == null)
            return;

        for (int i = 0; i < routes.Length; i++)
        {
            RouteConfig route = routes[i];
            if (route == null || route.interactable == null || !route.listenersBound)
                continue;

            if (route.hoverListener != null)
                TrySetWrapperListener(route.interactable, "WhenHover", route.hoverListener, add: false);

            if (route.unhoverListener != null)
                TrySetWrapperListener(route.interactable, "WhenUnhover", route.unhoverListener, add: false);

            route.listenersBound = false;
        }
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

    private void HandleRouteHover(int routeIndex)
    {
        if (!_interactionEnabled || _selectionLocked || routeIndex < 0 || routeIndex >= routes.Length)
            return;

        if (routeIndex == _activeRouteIndex)
            return;

        ClearActiveHold(resetMaterials: true);
        _activeRouteIndex = routeIndex;
        _holdElapsed = 0f;
        ShowIndicator(0f, fillColor);
        ApplyHoverMaterial(routeIndex);
    }

    private void HandleRouteUnhover(int routeIndex)
    {
        if (!_interactionEnabled || _selectionLocked)
            return;

        if (routeIndex != _activeRouteIndex)
            return;

        ClearActiveHold(resetMaterials: true);
    }

    private void ClearActiveHold(bool resetMaterials)
    {
        HideIndicator();

        _activeRouteIndex = -1;
        _holdElapsed = 0f;

        if (resetMaterials)
            ResetRouteMaterials();
    }

    private void CommitSelection(int routeIndex)
    {
        if (routeIndex < 0 || routeIndex >= routes.Length)
            return;

        RouteConfig route = routes[routeIndex];
        if (route == null || route.routeObject == null)
            return;

        _selectionLocked = true;
        _interactionEnabled = false;
        _activeRouteIndex = routeIndex;
        _holdElapsed = holdDuration;

        ShowIndicator(1f, selectedFillColor);
        ApplySelectedMaterial(routeIndex);
        BindSelectorToPrisonerRoute();

        if (prisonerRoute != null)
        {
            prisonerRoute.AuthorizeRouteSelection(route.routeId);
            prisonerRoute.SelectRoute(route.routeId);
        }

        routeController?.SetDistanceHandGrabInteractorActive(false);
        HideIndicator();
        SetWrapperEnabled(false);
        _activeRouteIndex = -1;
        _holdElapsed = 0f;
    }

    private void ApplyHoverMaterial(int routeIndex)
    {
        for (int i = 0; i < routes.Length; i++)
        {
            Material mat = routes[i]?.routeMaterial;
            if (mat == null)
                continue;

            mat.SetColor("_EmissionColor", idleEmissionColor);
            mat.SetFloat("_EmissionStrength", i == routeIndex ? hoverEmissionStrength : idleEmissionStrength);
        }
    }

    private void ApplySelectedMaterial(int routeIndex)
    {
        for (int i = 0; i < routes.Length; i++)
        {
            Material mat = routes[i]?.routeMaterial;
            if (mat == null)
                continue;

            if (i == routeIndex)
            {
                mat.SetColor("_EmissionColor", selectedEmissionColor);
                mat.SetFloat("_EmissionStrength", selectedEmissionStrength);
            }
            else
            {
                mat.SetColor("_EmissionColor", idleEmissionColor);
                mat.SetFloat("_EmissionStrength", idleEmissionStrength);
            }
        }
    }

    private void ResetRouteMaterials()
    {
        for (int i = 0; i < routes.Length; i++)
        {
            Material mat = routes[i]?.routeMaterial;
            if (mat == null)
                continue;

            mat.SetColor("_EmissionColor", idleEmissionColor);
            mat.SetFloat("_EmissionStrength", idleEmissionStrength);
        }
    }

    private void SetWrapperEnabled(bool enabled)
    {
        for (int i = 0; i < routes.Length; i++)
        {
            if (routes[i]?.interactable == null)
                continue;

            routes[i].interactable.enabled = enabled;
        }
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

    private static string ResolveRouteId(params string[] names)
    {
        foreach (string rawName in names)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                continue;

            string normalized = rawName.ToLowerInvariant();
            if (normalized.Contains("cage") || normalized.Contains("cell"))
                return "cell";
            if (normalized.Contains("truck"))
                return "truck";
            if (normalized.Contains("ship") || normalized.Contains("boat"))
                return "boat";
        }

        return string.Empty;
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
}
