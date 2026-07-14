using UnityEngine;

public class ParentVelocityToWalkAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator prisonerAnimator;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string walkPlaybackParameterName = "WalkPlayback";
    [SerializeField] private string turnLeftTriggerName = "TurnLeft";
    [SerializeField] private string turnRightTriggerName = "TurnRight";

    [Header("Speed Tuning")]
    [Tooltip("The real-world movement speed that visually matches the walk clip at 1x playback.")]
    [SerializeField] private float authoredWalkSpeed = 1.2f;

    [Tooltip("Below this speed, the character is considered idle.")]
    [SerializeField] private float idleThreshold = 0.08f;

    [Tooltip("Smooths the Animator speed parameter to avoid flickering between idle and walk.")]
    [SerializeField] private float speedDampTime = 0.12f;

    [Header("Playback Clamp")]
    [SerializeField] private float minWalkPlayback = 0.65f;
    [SerializeField] private float maxWalkPlayback = 1.35f;

    [Header("Turn Detection")]
    [Tooltip("The character must be at or below this ground speed before an in-place turn can trigger.")]
    [Min(0f)]
    [SerializeField] private float maximumMovementSpeedDuringTurn = 0.1f;

    [Tooltip("The minimum absolute parent yaw speed required to trigger a turn animation.")]
    [Min(0f)]
    [SerializeField] private float turnEnterYawSpeed = 20f;

    [Tooltip("The parent yaw speed must fall to or below this value before another turn can trigger.")]
    [Min(0f)]
    [SerializeField] private float turnExitYawSpeed = 5f;

    private Vector3 previousPosition;
    private float previousYaw;
    private bool turnDetectionArmed;

    private int speedParameter;
    private int walkPlaybackParameter;
    private int turnLeftTrigger;
    private int turnRightTrigger;

    private void Awake()
    {
        speedParameter = Animator.StringToHash(speedParameterName);
        walkPlaybackParameter = Animator.StringToHash(walkPlaybackParameterName);
        turnLeftTrigger = Animator.StringToHash(turnLeftTriggerName);
        turnRightTrigger = Animator.StringToHash(turnRightTriggerName);
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        previousYaw = transform.eulerAngles.y;
        turnDetectionArmed = true;
    }

    private void LateUpdate()
    {
        if (prisonerAnimator == null)
            return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - previousPosition;
        previousPosition = currentPosition;

        float currentYaw = transform.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);
        float yawSpeed = yawDelta / deltaTime;
        previousYaw = currentYaw;

        // Ignore vertical motion. We only care about ground-plane travel speed.
        delta.y = 0f;

        float horizontalSpeed = delta.magnitude / deltaTime;

        float animatorSpeed = horizontalSpeed < idleThreshold ? 0f : horizontalSpeed;

        float playback = authoredWalkSpeed > 0f
            ? horizontalSpeed / authoredWalkSpeed
            : 1f;

        playback = Mathf.Clamp(playback, minWalkPlayback, maxWalkPlayback);

        prisonerAnimator.SetFloat(speedParameter, animatorSpeed, speedDampTime, Time.deltaTime);
        prisonerAnimator.SetFloat(walkPlaybackParameter, playback, speedDampTime, Time.deltaTime);

        UpdateTurn(horizontalSpeed, yawSpeed);
    }

    private void UpdateTurn(float horizontalSpeed, float yawSpeed)
    {
        float absoluteYawSpeed = Mathf.Abs(yawSpeed);

        if (!turnDetectionArmed)
        {
            if (absoluteYawSpeed <= turnExitYawSpeed)
                turnDetectionArmed = true;

            return;
        }

        if (horizontalSpeed > maximumMovementSpeedDuringTurn ||
            absoluteYawSpeed < turnEnterYawSpeed)
        {
            return;
        }

        if (yawSpeed > 0f)
        {
            prisonerAnimator.ResetTrigger(turnLeftTrigger);
            prisonerAnimator.SetTrigger(turnRightTrigger);
        }
        else
        {
            prisonerAnimator.ResetTrigger(turnRightTrigger);
            prisonerAnimator.SetTrigger(turnLeftTrigger);
        }

        turnDetectionArmed = false;
    }
}
