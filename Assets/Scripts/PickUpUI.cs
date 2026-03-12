using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using Oculus.Interaction;

[DisallowMultipleComponent]
public class PickUpUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("World-space canvas containing the info UI")]
    [SerializeField] private Canvas infoCanvas;

    [Tooltip("Text component used to display info")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Tooltip("Position of the canvas")]
    public Vector3 offset = new Vector3(0, 0.15f, 0);

    [Header("Display Settings")]
    [TextArea]
    [SerializeField] private string displayText = "Object Info Here";

    [Tooltip("Vertical offset above the object")]
    [SerializeField] private float heightOffset = 0.15f;

    [Tooltip("Fade animation duration")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Tooltip("Should the UI always face the camera?")]
    [SerializeField] private bool faceCamera = true;

    [Header("Meta Interaction")]
    [SerializeField] private Grabbable grabbable;
    
    [Header("Events")]
    public UnityEvent onShow;
    public UnityEvent onHide;

    private CanvasGroup canvasGroup;
    private Transform cameraTransform;
    private Coroutine fadeCoroutine;
    private bool isHeld;

    private void Awake()
    {
        if (infoCanvas == null || infoText == null)
        {
            Debug.LogError("[PickupUI] Missing references");
            enabled = false;
            return;
        }

        canvasGroup = infoCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = infoCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        infoText.text = displayText;

        canvasGroup.alpha = 0f;
        infoCanvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;

        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += HandleGrabEvent;
        }
        else
        {
            Debug.LogWarning("[PickUpUI] No Grabbable component found");
        }
    }

    private void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= HandleGrabEvent;
        }
    }

    private void LateUpdate()
    {
        if (!isHeld) return;
        {
            PositionUI();
        }
    }

    private void PositionUI()
    {
        Renderer rend = GetComponent<Renderer>();
        float halfHeight = rend != null ? rend.bounds.extents.y : 0.1f;

        infoCanvas.transform.position = transform.position + transform.up * (halfHeight + heightOffset);
    }

    private void HandleGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            OnPickedUp();
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            OnReleased();
        }
    }

    public void OnPickedUp()
    {
        onShow?.Invoke();
        isHeld = true;

        infoCanvas.gameObject.SetActive(true);
        StartFade(1f);
    }

    public void OnReleased()
    {
        onHide?.Invoke();
        isHeld = false;
        StartCoroutine(HideRoutine());
    }

    private void StartFade(float target)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(Fade(canvasGroup.alpha, target));
    }

    private IEnumerator HideRoutine()
    {
        yield return Fade(canvasGroup.alpha, 0f);
        infoCanvas.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
