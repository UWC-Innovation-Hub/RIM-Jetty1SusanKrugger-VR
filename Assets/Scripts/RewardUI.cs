using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;

public class SpinningVRUIUniversal : MonoBehaviour, IPointerClickHandler
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private Vector3 spinAxis = Vector3.forward;
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private bool isSpinning = false;
    private float spinTimer = 0f;

    private PointableUnityEventWrapper wrapper;

    private void Awake()
    {
        // Setup XR Interactable for hand poke
        wrapper = GetComponent<PointableUnityEventWrapper>();
        if (wrapper == null)
        {
            wrapper = gameObject.AddComponent<PointableUnityEventWrapper>();
        }
    }

    private void Start()
    {
       if (wrapper != null)
        {
            wrapper.WhenSelect.AddListener(OnMetaInteract);
        }
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
    private void OnMetaInteract(PointerEvent pointerEvent)
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
        if (wrapper != null)
        {
            wrapper.WhenSelect.RemoveListener(OnMetaInteract);
        }
    }
}