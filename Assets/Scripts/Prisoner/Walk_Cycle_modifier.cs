using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpeedToAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform target; // Object to track (defaults to this)

    [Header("Speed Settings")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.1f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float speedVelocity; // for SmoothDamp

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (target == null)
            target = transform;

        lastPosition = target.position;
    }

    void Update()
    {
        // Calculate raw speed
        float distance = Vector3.Distance(target.position, lastPosition);
        float rawSpeed = distance / Time.deltaTime;

        // Smooth it
        currentSpeed = Mathf.SmoothDamp(currentSpeed, rawSpeed, ref speedVelocity, smoothTime);

        // Normalize (optional, keeps things predictable)
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        // Apply to animator
        animator.speed = normalizedSpeed * speedMultiplier;

        // Store position for next frame
        lastPosition = target.position;
    }
}