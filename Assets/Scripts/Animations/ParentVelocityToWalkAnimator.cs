using UnityEngine;
using UnityEngine.Serialization;

public class ParentVelocityToWalkAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator prisonerAnimator;

    [Header("Turn Rotation Source")]
    [Tooltip("When enabled, turn detection reads local yaw from Turn Rotation Source while movement speed continues to use this GameObject's world position.")]
    [SerializeField] private bool useSeparateTurnRotationSource = false;

    [Tooltip("The child transform whose local yaw drives turn detection when Use Separate Turn Rotation Source is enabled.")]
    [SerializeField] private Transform turnRotationSource;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string walkPlaybackParameterName = "WalkPlayback";
    [SerializeField] private string turnLeftTriggerName = "TurnLeft";
    [SerializeField] private string turnRightTriggerName = "TurnRight";
    [SerializeField] private string isTurningParameterName = "IsTurning";

    [Header("Speed Tuning")]
    [Tooltip("The real-world movement speed that visually matches the walk clip at 1x playback.")]
    [SerializeField] private float authoredWalkSpeed = 1.2f;

    [Tooltip("Below this speed, the character is considered idle.")]
    [SerializeField] private float idleThreshold = 0.08f;

    [Tooltip("Smooths the Speed parameter used by the Idle/Walk state transitions.")]
    [Min(0f)]
    [SerializeField] private float speedDampTime = 0.06f;

    [Tooltip("Keeps the Walk state eligible briefly after movement stops so an immediately following turn can take priority. Walk playback still follows real movement speed during this window.")]
    [Min(0f)]
    [FormerlySerializedAs("idleDecisionDelay")]
    [SerializeField] private float turnDecisionWindow = 0.3f;

    [Header("Playback Clamp")]
    [Tooltip("Minimum Walk clip playback multiplier. This prevents unnaturally slow walk cycles at low movement speeds.")]
    [Min(0f)]
    [SerializeField] private float minWalkPlayback = 0.65f;

    [Tooltip("Maximum Walk clip playback multiplier.")]
    [Min(0f)]
    [SerializeField] private float maxWalkPlayback = 1.35f;

    [Tooltip("Smooths WalkPlayback independently from the state-transition Speed parameter.")]
    [Min(0f)]
    [SerializeField] private float walkPlaybackDampTime = 0.03f;

    [Header("Turn Detection")]
    [Tooltip("The character must be at or below this ground speed before an in-place turn can trigger.")]
    [Min(0f)]
    [SerializeField] private float maximumMovementSpeedDuringTurn = 0.1f;

    [Tooltip("The minimum absolute yaw speed required to trigger a turn animation.")]
    [Min(0f)]
    [SerializeField] private float turnEnterYawSpeed = 20f;

    [Tooltip("The yaw speed must fall to or below this value before another turn can trigger.")]
    [Min(0f)]
    [SerializeField] private float turnExitYawSpeed = 5f;

    private Vector3 previousPosition;
    private float previousYaw;
    private bool turnDetectionArmed;
    private bool wasMoving;
    private float turnDecisionElapsed;
    private Transform activeTurnRotationSource;
    private bool activeTurnRotationUsesLocalYaw;
    private bool missingTurnRotationSourceWarningIssued;

    private int speedParameter;
    private int walkPlaybackParameter;
    private int turnLeftTrigger;
    private int turnRightTrigger;
    private int isTurningParameter;

    private void Awake()
    {
        speedParameter = Animator.StringToHash(speedParameterName);
        walkPlaybackParameter = Animator.StringToHash(walkPlaybackParameterName);
        turnLeftTrigger = Animator.StringToHash(turnLeftTriggerName);
        turnRightTrigger = Animator.StringToHash(turnRightTriggerName);
        isTurningParameter = Animator.StringToHash(isTurningParameterName);
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        missingTurnRotationSourceWarningIssued = false;
        activeTurnRotationSource = GetTurnRotationSource(out activeTurnRotationUsesLocalYaw);
        previousYaw = GetYaw(activeTurnRotationSource, activeTurnRotationUsesLocalYaw);
        turnDetectionArmed = true;
        wasMoving = false;
        turnDecisionElapsed = 0f;

        if (prisonerAnimator != null)
            prisonerAnimator.SetBool(isTurningParameter, false);
    }

    private void OnDisable()
    {
        if (prisonerAnimator == null)
            return;

        prisonerAnimator.SetBool(isTurningParameter, false);
        prisonerAnimator.ResetTrigger(turnLeftTrigger);
        prisonerAnimator.ResetTrigger(turnRightTrigger);
    }

    private void LateUpdate()
    {
        if (prisonerAnimator == null)
            return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - previousPosition;
        previousPosition = currentPosition;

        float yawSpeed = GetYawSpeed(deltaTime);

        // Ignore vertical motion. We only care about ground-plane travel speed.
        delta.y = 0f;

        float horizontalSpeed = delta.magnitude / deltaTime;

        UpdateTurn(horizontalSpeed, yawSpeed);

        float animatorSpeed = GetAnimatorSpeed(horizontalSpeed, deltaTime);

        float playback = authoredWalkSpeed > 0f
            ? horizontalSpeed / authoredWalkSpeed
            : 1f;

        playback = Mathf.Clamp(playback, minWalkPlayback, maxWalkPlayback);

        prisonerAnimator.SetFloat(speedParameter, animatorSpeed, speedDampTime, deltaTime);
        prisonerAnimator.SetFloat(walkPlaybackParameter, playback, walkPlaybackDampTime, deltaTime);
    }

    private float GetYawSpeed(float deltaTime)
    {
        Transform currentTurnRotationSource =
            GetTurnRotationSource(out bool currentTurnRotationUsesLocalYaw);

        float currentYaw = GetYaw(
            currentTurnRotationSource,
            currentTurnRotationUsesLocalYaw);

        if (currentTurnRotationSource != activeTurnRotationSource ||
            currentTurnRotationUsesLocalYaw != activeTurnRotationUsesLocalYaw)
        {
            activeTurnRotationSource = currentTurnRotationSource;
            activeTurnRotationUsesLocalYaw = currentTurnRotationUsesLocalYaw;
            previousYaw = currentYaw;
            return 0f;
        }

        float yawDelta = Mathf.DeltaAngle(previousYaw, currentYaw);
        previousYaw = currentYaw;
        return yawDelta / deltaTime;
    }

    private Transform GetTurnRotationSource(out bool useLocalYaw)
    {
        useLocalYaw = useSeparateTurnRotationSource && turnRotationSource != null;

        if (useLocalYaw)
            return turnRotationSource;

        if (useSeparateTurnRotationSource &&
            !missingTurnRotationSourceWarningIssued)
        {
            Debug.LogWarning(
                $"{nameof(ParentVelocityToWalkAnimator)} on '{name}' has " +
                $"{nameof(useSeparateTurnRotationSource)} enabled without a " +
                $"{nameof(turnRotationSource)}. Falling back to this GameObject's world yaw.",
                this);

            missingTurnRotationSourceWarningIssued = true;
        }

        return transform;
    }

    private static float GetYaw(Transform rotationSource, bool useLocalYaw)
    {
        return useLocalYaw
            ? rotationSource.localEulerAngles.y
            : rotationSource.eulerAngles.y;
    }

    private float GetAnimatorSpeed(float horizontalSpeed, float deltaTime)
    {
        if (horizontalSpeed >= idleThreshold)
        {
            wasMoving = true;
            turnDecisionElapsed = 0f;
            return horizontalSpeed;
        }

        if (!turnDetectionArmed)
        {
            wasMoving = false;
            turnDecisionElapsed = 0f;
            return 0f;
        }

        if (!wasMoving)
            return 0f;

        turnDecisionElapsed += deltaTime;

        if (turnDecisionElapsed < turnDecisionWindow)
            return idleThreshold + 0.001f;

        wasMoving = false;
        turnDecisionElapsed = 0f;
        return 0f;
    }

    private void UpdateTurn(float horizontalSpeed, float yawSpeed)
    {
        float absoluteYawSpeed = Mathf.Abs(yawSpeed);

        if (!turnDetectionArmed)
        {
            if (absoluteYawSpeed <= turnExitYawSpeed)
            {
                turnDetectionArmed = true;
                prisonerAnimator.SetBool(isTurningParameter, false);
            }

            return;
        }

        if (horizontalSpeed > maximumMovementSpeedDuringTurn ||
            absoluteYawSpeed < turnEnterYawSpeed)
        {
            return;
        }

        prisonerAnimator.SetBool(isTurningParameter, true);

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
