using UnityEngine;

public class CameraSequenceController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 30f;
    public bool invertDirection = false;

    [Header("Movement Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;

    [Header("Control")]
    public bool playOnStart = false;

    private bool isPlaying = false;
    private int currentWaypointIndex = 0;

    // ✅ Store initial transform
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // Save original state
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (playOnStart)
        {
            StartSequence();
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        RotateCamera();
        MoveCamera();
    }

    void RotateCamera()
    {
        float direction = invertDirection ? -1f : 1f;

        transform.Rotate(
            rotationAxis.normalized * rotationSpeed * direction * Time.deltaTime,
            Space.Self
        );
    }

    void MoveCamera()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex++;

            // 🔁 End reached → FULL RESET LOOP
            if (currentWaypointIndex >= waypoints.Length)
            {
                ResetToStart();
            }
        }
    }

    void ResetToStart()
    {
        // Reset transform to original state
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Restart path
        currentWaypointIndex = 0;
    }

    public void StartSequence()
    {
        // Ensure clean start every time
        ResetToStart();
        isPlaying = true;
    }

    public void StopSequence()
    {
        isPlaying = false;
    }
}