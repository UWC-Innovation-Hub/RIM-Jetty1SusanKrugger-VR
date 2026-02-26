using UnityEngine;

[DisallowMultipleComponent]
public class GazeRaycaster : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera used for gaze direction (usually the VR headset camera).")]
    [SerializeField] private Camera gazeCamera;

    [Header("Raycast Settings")]
    [Tooltip("Maximum gaze distance")]
    [SerializeField] private float maxDistance = 10f;

    [Tooltip("Which layers can be gazed at.")]
    [SerializeField] private LayerMask interactableLayers;

    [Header("Timing")]
    [Tooltip("Time (seconds) required to trigger a gaze action")]
    [SerializeField] private float dwellTime = 1f;

    private IGazeTarget currentTarget;
    private float gazeTimer;

    private void Reset()
    {
        gazeCamera = Camera.main;
    }

    private void Awake()
    {
        if (gazeCamera == null)
        {
            gazeCamera = Camera.main;
        }

        if (gazeCamera == null)
        {
            Debug.LogError("[GazeRaycaster] No camera assigned");
        }
    }

    private void Update()
    {
        PeformRaycast();
    }

    private void PeformRaycast()
    {
        if (gazeCamera == null) return;

        Ray ray = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayers))
        {
            IGazeTarget target = hit.collider.GetComponent<IGazeTarget>();

            if (target != null)
            {
                HandleTarget(target);
                return;
            }
        }

        ClearTarget();
    }

    private void HandleTarget(IGazeTarget newTarget)
    {
        if (newTarget != currentTarget)
        {
            ClearTarget();
            currentTarget = newTarget;
            currentTarget.OnGazeEnter();
            gazeTimer = 0f;
        }

        gazeTimer += Time.deltaTime;

        if (gazeTimer >= dwellTime)
        {
            currentTarget.OnGazeDwell();
        }
    }

    private void ClearTarget()
    {
        if (currentTarget == null) return;

        currentTarget.OnGazeExit();
        currentTarget = null;
        gazeTimer = 0f;
    }
}
