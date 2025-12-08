using UnityEngine;
using UnityEngine.Events;

public class ClapDetector : MonoBehaviour
{
    [Header("Hand Tracking")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Clap Settings")]
    public float clapDistance = 0.15f;          // Max distance for clap
    public float minImpactVelocity = 0.4f;      // Minimum inward speed
    public float clapCooldown = 0.5f;           // Cooldown between claps

    [Header("Smoothing")]
    public float velocitySmoothing = 0.15f;     // Smoother velocity
    public float positionSmoothing = 0.1f;      // Optional hand smoothing

    [Header("Events")]
    public UnityEvent OnClap;

    private float lastClapTime = -1f;
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private Vector3 leftVelSmoothed;
    private Vector3 rightVelSmoothed;

    void Start()
    {
        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
    }

    void Update()
    {
        if (leftHand == null || rightHand == null) return;
        if (!leftHand.gameObject.activeInHierarchy || !rightHand.gameObject.activeInHierarchy) return;

        Vector3 leftPos = Vector3.Lerp(prevLeftPos, leftHand.position, positionSmoothing);
        Vector3 rightPos = Vector3.Lerp(prevRightPos, rightHand.position, positionSmoothing);

        float dist = Vector3.Distance(leftPos, rightPos);

        // Instant velocities
        Vector3 leftVel = (leftPos - prevLeftPos) / Time.deltaTime;
        Vector3 rightVel = (rightPos - prevRightPos) / Time.deltaTime;

        // Smooth velocities
        leftVelSmoothed = Vector3.Lerp(leftVelSmoothed, leftVel, velocitySmoothing);
        rightVelSmoothed = Vector3.Lerp(rightVelSmoothed, rightVel, velocitySmoothing);

        // Impact velocity
        Vector3 relativeVel = leftVelSmoothed - rightVelSmoothed;
        float inwardSpeed = relativeVel.magnitude;

        // Clap detection
        if (dist < clapDistance &&
            inwardSpeed > minImpactVelocity &&
            Time.time - lastClapTime > clapCooldown)
        {
            lastClapTime = Time.time;
            OnClap.Invoke();
        }

        prevLeftPos = leftPos;
        prevRightPos = rightPos;
    }

    public void LogClap()
    {
        Debug.Log("👏 Clap detected!");
    }

}
