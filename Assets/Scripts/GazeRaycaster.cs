using System.Collections.Generic;
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

    [Tooltip("Time (seconds) before the same target can be gazed again after exit.")]
    [SerializeField] private float reentryCooldown = 2f;

    private IGazeTarget currentTarget;
    private float gazeTimer;
    private bool hasTriggeredDwell;
    private readonly Dictionary<IGazeTarget, float> blockedTargets = new Dictionary<IGazeTarget, float>();

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

            if (target != null && !IsTargetBlocked(target))
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
            hasTriggeredDwell = false;
        }

        gazeTimer += Time.deltaTime;

        if (!hasTriggeredDwell && gazeTimer >= dwellTime)
        {
            currentTarget.OnGazeDwell();
            hasTriggeredDwell = true;
        }
    }

    private void ClearTarget()
    {
        if (currentTarget == null) return;

        blockedTargets[currentTarget] = Time.time + reentryCooldown;
        currentTarget.OnGazeExit();
        currentTarget = null;
        gazeTimer = 0f;
        hasTriggeredDwell = false;
    }

    private bool IsTargetBlocked(IGazeTarget target)
    {
        if (!blockedTargets.TryGetValue(target, out float blockedUntil))
        {
            return false;
        }

        if (Time.time >= blockedUntil)
        {
            blockedTargets.Remove(target);
            return false;
        }

        return true;
    }
}
