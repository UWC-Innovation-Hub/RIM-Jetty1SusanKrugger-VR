using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SpinningVRUIUniversal : MonoBehaviour, IPointerClickHandler
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private Vector3 spinAxis = Vector3.forward;
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private bool isSpinning = false;
    private float spinTimer = 0f;
    private XRSimpleInteractable xrInteractable;

    private void Awake()
    {
        // Setup XR Interactable for hand poke
        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
        {
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void Start()
    {
        // Listen for XR interactions (poke)
        xrInteractable.selectEntered.AddListener(OnXRInteract);
    }

    private void Update()
    {
        if (isSpinning)
        {
            spinTimer += Time.deltaTime;
            float curveValue = spinCurve.Evaluate(Mathf.Clamp01(spinTimer / spinDuration));

            transform.Rotate(spinAxis, spinSpeed * curveValue * Time.deltaTime, Space.Self);

            if (spinTimer >= spinDuration)
            {
                StopSpinning();
            }
        }
    }

    // For ray-based controller interaction
    public void OnPointerClick(PointerEventData eventData)
    {
        StartSpinning();
    }

    // For hand poke interaction
    private void OnXRInteract(SelectEnterEventArgs args)
    {
        StartSpinning();
    }

    private void StartSpinning()
    {
        isSpinning = true;
        spinTimer = 0f;
    }

    private void StopSpinning()
    {
        isSpinning = false;
        spinTimer = 0f;
    }

    private void OnDestroy()
    {
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnXRInteract);
        }
    }
}