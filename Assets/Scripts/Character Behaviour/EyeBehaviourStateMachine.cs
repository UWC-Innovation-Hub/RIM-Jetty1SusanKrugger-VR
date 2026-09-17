using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EyeBehaviorStateMachine : MonoBehaviour
{
    public enum EyeState
    {
        Idle,
        EyeContact,
        Stare
    }

    public enum LocalAxis
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }

    /// <summary>
    /// A single weighted Point of Interest that Idle can occasionally choose
    /// to look at instead of generating a procedural scan target.
    /// </summary>
    [Serializable]
    public class InterestPoint
    {
        [Tooltip("The Transform this interest point represents.")]
        public Transform target;

        [Tooltip(
            "Relative selection weight, not a direct percentage. " +
            "0 = never selected. Two points both set to 1 are equally likely. " +
            "A point set to 0.5 is half as likely as a point set to 1.")]
        [Range(0f, 1f)]
        public float weight = 1f;
    }

    [Header("Eye References")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;

    [Header("Left Eye Axis Mapping")]
    [Tooltip("The local axis of the left eye object that physically points forward.")]
    [SerializeField] private LocalAxis leftForwardAxis = LocalAxis.PositiveZ;

    [Tooltip("The local axis of the left eye object that physically points upward.")]
    [SerializeField] private LocalAxis leftUpAxis = LocalAxis.PositiveY;

    [Header("Right Eye Axis Mapping")]
    [Tooltip("The local axis of the right eye object that physically points forward.")]
    [SerializeField] private LocalAxis rightForwardAxis = LocalAxis.PositiveZ;

    [Tooltip("The local axis of the right eye object that physically points upward.")]
    [SerializeField] private LocalAxis rightUpAxis = LocalAxis.PositiveY;

    [Header("Angular Clamp")]
    [Tooltip("Maximum horizontal eye rotation left/right from the calibrated rest direction.")]
    [Range(0f, 90f)]
    [SerializeField] private float horizontalClamp = 40f;

    [Tooltip("Maximum vertical eye rotation up/down from the calibrated rest direction.")]
    [Range(0f, 90f)]
    [SerializeField] private float verticalClamp = 25f;

    [Header("General")]
    [Tooltip(
        "Smooth time used when transitioning the shared world-space look point. " +
        "This is separate from state-specific movement speeds. Lower values react faster.")]
    [Min(0.001f)]
    public float stateBlendTime = 0.08f;

    [Tooltip("Distance used when generating procedural look points.")]
    [Min(0.1f)]
    [SerializeField] private float proceduralLookDistance = 5f;

    [Header("Look Point Reference")]

    [Tooltip(
        "Optional empty GameObject. When assigned, it is repositioned to the eyes' " +
        "actual gaze point whenever that point deviates far enough from straight-ahead " +
        "(per the trigger distances below). Useful as an aim target for a head/spine " +
        "Multi-Aim Constraint, so the head only reacts to meaningfully large eye " +
        "movements rather than every small idle scan.")]
    [SerializeField] private Transform lookPointReference;

    [Tooltip(
        "Minimum horizontal deviation (world units, measured perpendicular to the " +
        "character's straight-ahead direction) the gaze point must reach before " +
        "Look Point Reference is moved to it.")]
    [Min(0f)]
    [SerializeField] private float lookPointHorizontalTriggerDistance = 0.3f;

    [Tooltip(
        "Minimum vertical deviation (world units, measured perpendicular to the " +
        "character's straight-ahead direction) the gaze point must reach before " +
        "Look Point Reference is moved to it.")]
    [Min(0f)]
    [SerializeField] private float lookPointVerticalTriggerDistance = 0.3f;

    [Header("Look Movement - Jitter")]

    [Tooltip(
        "Probability that a NEW look destination (a fresh Idle target, an Eye " +
        "Contact stare/look-away swap, a new Stare target, etc.) is approached via " +
        "a staggered/jittered path instead of moving directly. Rolled once per " +
        "destination change, never every frame. 0 = eyes never jitter point to " +
        "point. 1 = eyes always jitter point to point.")]
    [Range(0f, 1f)]
    [SerializeField] private float jitterProbability = 0f;

    [Tooltip(
        "Relative density of jitter waypoints per unit of world-space distance " +
        "between the old and new look point. Longer transitions get proportionally " +
        "more jitter waypoints than short ones (clamped to a sane range).")]
    [Min(0f)]
    [SerializeField] private float jitterAmount = 1f;

    [Tooltip(
        "How far each intermediate jitter waypoint deviates from the straight line " +
        "drawn between the old and new point, scaled by the transition distance. " +
        "0 = waypoints sit essentially on the direct line. Higher values scatter " +
        "them further off that line.")]
    [Range(0f, 1f)]
    [SerializeField] private float jitterRandomness = 0.3f;

    // -------------------------------------------------------------------------
    // Idle
    // -------------------------------------------------------------------------

    [Header("Idle - Behaviour")]

    [Tooltip("0 = eyes stay centered/dead ahead. 1 = eyes constantly choose scanning points.")]
    [Range(0f, 1f)]
    [SerializeField] private float scanning = 0.7f;

    [Tooltip(
        "0 = eyes barely move toward newly selected scan points. " +
        "1 = eyes move toward them almost immediately.")]
    [Range(0f, 1f)]
    [SerializeField] private float eyeMoveSpeed = 0.65f;

    [Tooltip(
        "0 = eyes linger for a long time at each point. " +
        "1 = eyes pause only briefly.")]
    [Range(0f, 1f)]
    [SerializeField] private float scanningSpeed = 0.5f;

    [Tooltip(
        "0 = scan hold duration is predictable. " +
        "1 = hold duration is strongly randomized.")]
    [Range(0f, 1f)]
    [SerializeField] private float scanSpeedRandomness = 0.5f;

    [Tooltip(
        "0 = scan points remain close to center. " +
        "1 = horizontal scan points may theoretically reach \u00b1180 degrees. " +
        "The final eye clamp still limits the physical eye rotation.")]
    [Range(0f, 1f)]
    [SerializeField] private float scanningSize = 0.25f;

    [Tooltip(
        "0 = vertical scanning is limited to approximately \u00b125 degrees. " +
        "1 = vertical scanning approaches its full useful range.")]
    [Range(0f, 1f)]
    [SerializeField] private float scanningHeight = 0.25f;

    [Header("Idle - Timing")]

    [Tooltip("Shortest possible pause between idle scan targets.")]
    [Min(0.01f)]
    [SerializeField] private float minimumIdleHoldTime = 0.15f;

    [Tooltip("Longest possible pause between idle scan targets.")]
    [Min(0.01f)]
    [SerializeField] private float maximumIdleHoldTime = 4f;

    [Header("Idle - Movement Speed")]

    [Tooltip("Target-following speed when Eye Move Speed is 0.")]
    [Min(0f)]
    [SerializeField] private float minimumIdleMoveSpeed = 0f;

    [Tooltip("Target-following speed when Eye Move Speed is 1.")]
    [Min(0f)]
    [SerializeField] private float maximumIdleMoveSpeed = 25f;

    [Header("Idle - Interest Points")]

    [Tooltip(
        "Probability that a newly chosen Idle target will come from the Interest Points " +
        "list instead of the procedural scan. 0 = never use interest points. " +
        "1 = always use one if a valid interest point exists. This is only rolled " +
        "at the moment a new Idle target is chosen, never every frame.")]
    [Range(0f, 1f)]
    [SerializeField] private float interestPointsInfluence = 0f;

    [Tooltip(
        "Maximum angle from the calibrated character forward direction for an " +
        "Interest Point to be eligible for selection. Points outside this angle, " +
        "or clearly behind the character, are ignored.")]
    [Range(1f, 180f)]
    [SerializeField] private float interestPointFieldOfView = 100f;

    [Tooltip(
        "Weighted list of points Idle may occasionally look at. Weight is relative, " +
        "not a direct percentage: two points both weighted 1 are equally likely, " +
        "a point weighted 0.5 is half as likely as a point weighted 1, and 0 disables it.")]
    [SerializeField] private List<InterestPoint> interestPoints = new List<InterestPoint>();

    // -------------------------------------------------------------------------
    // Eye Contact
    // -------------------------------------------------------------------------

    [Header("Eye Contact")]

    [Tooltip(
        "How long the character normally looks directly at the target. " +
        "0 = the character never directly looks at the target.")]
    [Min(0f)]
    [SerializeField] private float stareTime = 3f;

    [Tooltip(
        "0 = stare duration always uses Stare Time. " +
        "1 = stare duration can vary between Min Stare Time and Stare Time.")]
    [Range(0f, 1f)]
    [SerializeField] private float stareTimeRandomness = 0.4f;

    [Tooltip("Minimum possible direct stare duration when randomness is enabled.")]
    [Min(0f)]
    [SerializeField] private float minimumStareTime = 1f;

    [Tooltip(
        "How long the character glances away before returning to the target. " +
        "0 = the character never looks away.")]
    [Min(0f)]
    [SerializeField] private float lookAwayTime = 0.8f;

    [Tooltip("Maximum horizontal angle used when choosing an eye-contact glance-away point.")]
    [Range(0f, 90f)]
    [SerializeField] private float lookAwayHorizontalRange = 15f;

    [Tooltip("Maximum vertical angle used when choosing an eye-contact glance-away point.")]
    [Range(0f, 90f)]
    [SerializeField] private float lookAwayVerticalRange = 10f;

    [Tooltip(
        "Maximum angle between the character's forward direction and an Eye Contact target. " +
        "Targets beyond this are temporarily ignored.")]
    [Range(1f, 90f)]
    [SerializeField] private float eyeContactFieldOfView = 80f;

    [Header("Reactive Eye Contact")]

    [Tooltip(
        "When enabled, Idle can automatically transition into Eye Contact based on the " +
        "target's distance and the Reactive Eye Contact probability curve. This never " +
        "overrides an explicitly requested EyeContact or Stare state.")]
    [SerializeField] private bool reactiveEyeContact = false;

    [Tooltip(
        "Distance from the eyes to the Eye Contact target that maps to X = 0 on the " +
        "Reactive Eye Contact probability curve.")]
    [Min(0f)]
    [SerializeField] private float reactiveEyeContactMinDistance = 0.5f;

    [Tooltip(
        "Distance from the eyes to the Eye Contact target that maps to X = 1 on the " +
        "Reactive Eye Contact probability curve.")]
    [Min(0f)]
    [SerializeField] private float reactiveEyeContactMaxDistance = 5f;

    [Tooltip(
        "Maps normalized target distance (X: 0 = Min Distance, 1 = Max Distance) to the " +
        "probability of initiating reactive eye contact (Y: 0 = never, 1 = always). " +
        "Shape this to taste, e.g. moderate at very close range, strong at conversational " +
        "distance, and low far away.")]
    [SerializeField]
    private AnimationCurve reactiveEyeContactCurve =
        AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Tooltip("Shortest possible time between Reactive Eye Contact decisions.")]
    [Min(0.01f)]
    [SerializeField] private float reactiveDecisionMinTime = 1f;

    [Tooltip("Longest possible time between Reactive Eye Contact decisions.")]
    [Min(0.01f)]
    [SerializeField] private float reactiveDecisionMaxTime = 3f;

    [Header("Runtime Target")]
    [SerializeField] private Transform eyeContactTarget;

    [Header("Runtime State")]
    [SerializeField] private EyeState currentState = EyeState.Idle;

    // -------------------------------------------------------------------------
    // Runtime calibration
    // -------------------------------------------------------------------------

    private Quaternion leftRestLocalRotation;
    private Quaternion rightRestLocalRotation;

    private Quaternion leftAxisBasis;
    private Quaternion rightAxisBasis;

    // -------------------------------------------------------------------------
    // Shared look point
    // -------------------------------------------------------------------------

    private Vector3 currentLookPoint;
    private Vector3 desiredLookPoint;
    private Vector3 lookPointVelocity;

    // -------------------------------------------------------------------------
    // Idle runtime
    // -------------------------------------------------------------------------

    private float stateTimer;

    private Vector3 idleTargetPoint;
    private Vector3 idleMovingPoint;

    /// <summary>
    /// Non-null while the current Idle target is a moving Interest Point,
    /// so UpdateIdle() can keep tracking it instead of a fixed position.
    /// Cleared whenever procedural scanning is chosen instead.
    /// </summary>
    private Transform currentIdleInterestTarget;

    // -------------------------------------------------------------------------
    // Eye contact runtime
    // -------------------------------------------------------------------------

    private bool eyeContactLookingAway;
    private Vector3 eyeContactAwayPoint;

    /// <summary>
    /// Set whenever EyeContact behaviour begins fresh (explicit state entry,
    /// Reactive Eye Contact engaging, or a new target assigned mid-EyeContact),
    /// so the very first look at the target/away-point is eligible for jitter.
    /// </summary>
    private bool eyeContactJustEntered;

    // -------------------------------------------------------------------------
    // Stare runtime
    // -------------------------------------------------------------------------

    /// <summary>
    /// Set whenever Stare begins fresh (explicit state entry, or a new target
    /// assigned mid-Stare), so the first look at the target is eligible for jitter.
    /// </summary>
    private bool stareJustEntered;

    // -------------------------------------------------------------------------
    // Reactive Eye Contact runtime
    // -------------------------------------------------------------------------

    /// <summary>
    /// True while Reactive Eye Contact is temporarily using the EyeContact
    /// logic even though the publicly requested state (currentState) is
    /// still Idle.
    /// </summary>
    private bool reactiveEyeContactEngaged;

    private float reactiveDecisionTimer;

    // -------------------------------------------------------------------------
    // Jitter runtime
    // -------------------------------------------------------------------------

    private const int MinJitterWaypoints = 1;
    private const int MaxJitterWaypoints = 6;
    private const float MinJitterDistance = 0.05f;
    private const float JitterWaypointArrivalTolerance = 0.02f;
    private const float JitterMaxWaypointTime = 0.35f;
    private const float JitterDeviationScale = 0.5f;

    // Fixed-size reusable buffer - never allocated per-transition.
    private readonly Vector3[] jitterWaypoints = new Vector3[MaxJitterWaypoints + 1];

    private bool jitterActive;
    private int jitterWaypointCount;
    private int jitterWaypointIndex;
    private float jitterWaypointTimer;

    /// <summary>
    /// Set true for exactly one frame when a jitter sequence finishes, so the
    /// calling state (Idle) can resync its own movement-speed system without
    /// a discontinuity.
    /// </summary>
    private bool jitterJustCompleted;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public event Action<EyeState> StateChanged;

    /// <summary>
    /// Current eye behaviour state.
    /// Setting this property is equivalent to calling SetState().
    /// </summary>
    public EyeState CurrentState
    {
        get => currentState;
        set => SetState(value);
    }

    /// <summary>
    /// Target used by both EyeContact and Stare.
    /// </summary>
    public Transform Target
    {
        get => eyeContactTarget;
        set => SetTarget(value);
    }

    /// <summary>
    /// Enables or disables Reactive Eye Contact at runtime.
    /// Equivalent to calling SetReactiveEyeContact().
    /// </summary>
    public bool ReactiveEyeContact
    {
        get => reactiveEyeContact;
        set => SetReactiveEyeContact(value);
    }

    public void SetState(EyeState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        // Reactive Eye Contact only ever operates while the publicly
        // requested state is Idle. An explicit Stare/EyeContact request
        // always takes priority and must not be interfered with.
        if (currentState != EyeState.Idle)
            reactiveEyeContactEngaged = false;

        ResetState();

        StateChanged?.Invoke(currentState);
    }

    public void SetTarget(Transform newTarget)
    {
        if (eyeContactTarget == newTarget)
            return;

        eyeContactTarget = newTarget;

        // Reset the state's logical timing, but deliberately DO NOT reset
        // currentLookPoint. The global smoothing layer therefore transitions
        // naturally to the new target rather than snapping.
        if (currentState == EyeState.EyeContact || reactiveEyeContactEngaged)
            ResetEyeContactState();

        if (currentState == EyeState.Stare)
            stareJustEntered = true;
    }

    /// <summary>
    /// Enables or disables Reactive Eye Contact at runtime. Disabling it
    /// gracefully cancels any currently engaged reactive eye contact.
    /// </summary>
    public void SetReactiveEyeContact(bool enabled)
    {
        reactiveEyeContact = enabled;

        if (!enabled)
            CancelReactiveEyeContact();
    }

    /// <summary>
    /// Returns the current curve-derived probability of Reactive Eye Contact
    /// engaging, based on the live distance to the Eye Contact target.
    /// Useful for debugging and other gameplay systems. Returns 0 if there
    /// is no valid target.
    /// </summary>
    public float GetReactiveEyeContactProbability()
    {
        if (eyeContactTarget == null)
            return 0f;

        float distance = Vector3.Distance(
            GetEyeCenter(),
            eyeContactTarget.position);

        float normalizedDistance = Mathf.Clamp01(
            Mathf.InverseLerp(
                reactiveEyeContactMinDistance,
                reactiveEyeContactMaxDistance,
                distance));

        float probability =
            reactiveEyeContactCurve.Evaluate(normalizedDistance);

        return Mathf.Clamp01(probability);
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError(
                $"{nameof(EyeBehaviorStateMachine)} on {name} requires both eye transforms.",
                this);

            enabled = false;
            return;
        }

        ValidateAxes();

        // Initial local rotations are treated as each eye's neutral/rest pose.
        leftRestLocalRotation = leftEye.localRotation;
        rightRestLocalRotation = rightEye.localRotation;

        // These convert the user-selected local forward/up axes into a
        // conventional basis where +Z = forward and +Y = up.
        leftAxisBasis = CreateAxisBasis(leftForwardAxis, leftUpAxis);
        rightAxisBasis = CreateAxisBasis(rightForwardAxis, rightUpAxis);

        currentState = EyeState.Idle;

        Vector3 center = GetEyeCenter();
        Vector3 forward = GetCharacterRestForward();

        currentLookPoint = center + forward * proceduralLookDistance;
        desiredLookPoint = currentLookPoint;

        idleTargetPoint = currentLookPoint;
        idleMovingPoint = currentLookPoint;

        reactiveDecisionTimer = UnityEngine.Random.Range(
            reactiveDecisionMinTime,
            reactiveDecisionMaxTime);

        ResetState();
    }

    // -------------------------------------------------------------------------
    // Main update
    // -------------------------------------------------------------------------

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // -------------------------------------------------------------
        // 1. Tick active state timer.
        // 2. Let that state update/generate its desired point.
        // -------------------------------------------------------------

        switch (currentState)
        {
            case EyeState.Idle:
                UpdateReactiveEyeContact(deltaTime);

                if (reactiveEyeContactEngaged)
                    UpdateEyeContact(deltaTime);
                else
                    UpdateIdle(deltaTime);
                break;

            case EyeState.EyeContact:
                UpdateEyeContact(deltaTime);
                break;

            case EyeState.Stare:
                UpdateStare(deltaTime);
                break;
        }

        // -------------------------------------------------------------
        // 3 / 4. Smooth ONE shared world-space look point.
        //
        // Every state feeds desiredLookPoint (optionally routed through a
        // jitter sequence first - see SetLookTarget()).
        // Neither eye receives a state target directly.
        //
        // This guarantees state and target changes go through the same
        // transition system.
        // -------------------------------------------------------------

        if (stateBlendTime <= 0.001f)
        {
            currentLookPoint = desiredLookPoint;
            lookPointVelocity = Vector3.zero;
        }
        else
        {
            currentLookPoint = Vector3.SmoothDamp(
                currentLookPoint,
                desiredLookPoint,
                ref lookPointVelocity,
                stateBlendTime,
                Mathf.Infinity,
                deltaTime);
        }

        // -------------------------------------------------------------
        // 5. Optionally reposition an external Look Point Reference object
        //    to the eyes' actual gaze point, once it deviates far enough
        //    from straight-ahead. Purely observational - never feeds back
        //    into eye rotation.
        // -------------------------------------------------------------

        UpdateLookPointReference();

        // -------------------------------------------------------------
        // 6. Independently rotate each eye toward the SAME world point.
        //
        // Because each eye has a different world-space position, they
        // naturally converge on nearby objects.
        // -------------------------------------------------------------

        RotateEye(
            leftEye,
            leftRestLocalRotation,
            leftAxisBasis);

        RotateEye(
            rightEye,
            rightRestLocalRotation,
            rightAxisBasis);
    }

    // =========================================================================
    // IDLE
    // =========================================================================

    private void UpdateIdle(float deltaTime)
    {
        // If Idle is currently following an Interest Point, keep re-reading
        // its live world position so moving points are tracked smoothly
        // instead of only being sampled once at selection time.
        if (currentIdleInterestTarget != null)
            idleTargetPoint = currentIdleInterestTarget.position;

        stateTimer -= deltaTime;

        bool choseNewTarget = false;

        if (stateTimer <= 0f)
        {
            ChooseNewIdleTarget();
            stateTimer = GetIdleHoldDuration();
            choseNewTarget = true;
        }

        if (!jitterActive)
        {
            float moveSpeed = Mathf.Lerp(
                minimumIdleMoveSpeed,
                maximumIdleMoveSpeed,
                eyeMoveSpeed);

            if (moveSpeed <= 0f)
            {
                // Eye Move Speed = 0 means do not travel between idle points.
                // idleMovingPoint simply remains where it currently is.
            }
            else
            {
                // Exponential smoothing gives a useful 0-1 "speed" control while
                // remaining frame-rate independent.
                float t = 1f - Mathf.Exp(-moveSpeed * deltaTime);

                idleMovingPoint = Vector3.Lerp(
                    idleMovingPoint,
                    idleTargetPoint,
                    t);
            }
        }

        // While an active jitter sequence is driving desiredLookPoint, keep
        // idleMovingPoint parked at the shared look point so that once the
        // sequence completes, normal Eye Move Speed control resumes from
        // the eyes' actual position with no discontinuity.
        SetLookTarget(
            idleMovingPoint,
            choseNewTarget ? idleTargetPoint : (Vector3?)null,
            deltaTime);

        if (jitterJustCompleted)
        {
            idleMovingPoint = idleTargetPoint;
            jitterJustCompleted = false;
        }
    }

    private void ChooseNewIdleTarget()
    {
        // Always start from "no interest point" and only set it back if
        // an interest point is actually selected below.
        currentIdleInterestTarget = null;

        if (interestPointsInfluence > 0f &&
            UnityEngine.Random.value <= interestPointsInfluence)
        {
            if (TrySelectInterestPoint(out Transform selected))
            {
                currentIdleInterestTarget = selected;
                idleTargetPoint = selected.position;
                return;
            }

            // No valid interest point available - fall back to the
            // existing procedural scan below.
        }

        Vector3 center = GetEyeCenter();
        Quaternion restBasis = GetCharacterRestBasis();

        // Scanning = 0 means dead ahead.
        if (scanning <= 0f)
        {
            idleTargetPoint =
                center +
                (restBasis * Vector3.forward) * proceduralLookDistance;

            return;
        }

        // Scanning controls how frequently we choose an actual off-center
        // location rather than resting at center.
        //
        // scanning = 1 -> every new target is a scan point.
        // scanning = 0 -> every new target is centered.
        if (UnityEngine.Random.value > scanning)
        {
            idleTargetPoint =
                center +
                (restBasis * Vector3.forward) * proceduralLookDistance;

            return;
        }

        float horizontalRange = Mathf.Lerp(
            0f,
            180f,
            scanningSize);

        /*
         * Assumption about "Scanning Height":
         *
         * The prompt describes 0 as a restricted vertical range and 1 as
         * matching the full horizontal range.
         *
         * Pitch values beyond \u00b190 degrees effectively begin pointing behind
         * the character, so treating \u00b1180 degrees as a useful vertical scan
         * range is ambiguous.
         *
         * Therefore:
         *     Scanning Height = 0 -> approximately \u00b125 degrees.
         *     Scanning Height = 1 -> up to \u00b190 degrees.
         *
         * The physical eye's Vertical Clamp is applied afterwards regardless.
         */

        float restrictedVerticalRange = 25f;

        float fullVerticalRange = Mathf.Min(
            90f,
            horizontalRange);

        float verticalRange = Mathf.Lerp(
            Mathf.Min(restrictedVerticalRange, horizontalRange),
            fullVerticalRange,
            scanningHeight);

        float yaw = UnityEngine.Random.Range(
            -horizontalRange,
            horizontalRange);

        float pitch = UnityEngine.Random.Range(
            -verticalRange,
            verticalRange);

        Vector3 direction =
            restBasis *
            Quaternion.Euler(-pitch, yaw, 0f) *
            Vector3.forward;

        idleTargetPoint =
            center +
            direction.normalized * proceduralLookDistance;
    }

    private float GetIdleHoldDuration()
    {
        float min = Mathf.Min(
            minimumIdleHoldTime,
            maximumIdleHoldTime);

        float max = Mathf.Max(
            minimumIdleHoldTime,
            maximumIdleHoldTime);

        // Scanning Speed:
        //
        // 0 -> base duration is the longest allowed hold.
        // 1 -> base duration is the shortest allowed hold.
        float baseDuration = Mathf.Lerp(
            max,
            min,
            scanningSpeed);

        if (scanSpeedRandomness <= 0f)
            return baseDuration;

        // Randomness expands the possible range outward from the
        // scan-speed-derived base value.
        //
        // randomness = 0:
        // minRandom = maxRandom = baseDuration
        //
        // randomness = 1:
        // full configured min/max range is available.
        float randomMin = Mathf.Lerp(
            baseDuration,
            min,
            scanSpeedRandomness);

        float randomMax = Mathf.Lerp(
            baseDuration,
            max,
            scanSpeedRandomness);

        return UnityEngine.Random.Range(
            randomMin,
            randomMax);
    }

    // =========================================================================
    // POINTS OF INTEREST
    // =========================================================================

    /// <summary>
    /// Performs weighted random selection over the configured Interest Points
    /// using two passes over the list (sum valid weight, then pick) so no
    /// temporary list is allocated.
    /// </summary>
    private bool TrySelectInterestPoint(out Transform selected)
    {
        selected = null;

        if (interestPoints == null || interestPoints.Count == 0)
            return false;

        float totalWeight = 0f;

        for (int i = 0; i < interestPoints.Count; i++)
        {
            if (IsInterestPointValid(interestPoints[i]))
                totalWeight += interestPoints[i].weight;
        }

        if (totalWeight <= 0f)
            return false;

        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < interestPoints.Count; i++)
        {
            InterestPoint point = interestPoints[i];

            if (!IsInterestPointValid(point))
                continue;

            cumulative += point.weight;

            if (roll <= cumulative)
            {
                selected = point.target;
                return true;
            }
        }

        // Floating point safety net: fall back to the last valid point
        // rather than reporting failure after a total weight was found.
        for (int i = interestPoints.Count - 1; i >= 0; i--)
        {
            if (IsInterestPointValid(interestPoints[i]))
            {
                selected = interestPoints[i].target;
                return true;
            }
        }

        return false;
    }

    private bool IsInterestPointValid(InterestPoint point)
    {
        if (point == null || point.target == null)
            return false;

        if (point.weight <= 0f)
            return false;

        Vector3 center = GetEyeCenter();
        Vector3 toPoint = point.target.position - center;

        if (toPoint.sqrMagnitude < 0.000001f)
            return true;

        toPoint.Normalize();

        // Use the calibrated rest forward direction rather than
        // transform.forward, matching the rest of the eye-targeting system.
        Vector3 forward = GetCharacterRestForward();

        if (Vector3.Dot(forward, toPoint) <= 0f)
            return false;

        float angle = Vector3.Angle(forward, toPoint);

        return angle <= interestPointFieldOfView;
    }

    // =========================================================================
    // REACTIVE EYE CONTACT
    // =========================================================================

    /// <summary>
    /// Runs the timer-driven Reactive Eye Contact decision system. Only
    /// called while the publicly requested state is Idle.
    /// </summary>
    private void UpdateReactiveEyeContact(float deltaTime)
    {
        if (!reactiveEyeContact)
        {
            if (reactiveEyeContactEngaged)
                CancelReactiveEyeContact();

            return;
        }

        if (reactiveEyeContactEngaged)
        {
            // Continuously re-validate the target while engaged so a lost
            // target results in a graceful, non-snapping return to Idle.
            if (eyeContactTarget == null || !IsEyeContactTargetValid())
                CancelReactiveEyeContact();

            return;
        }

        reactiveDecisionTimer -= deltaTime;

        if (reactiveDecisionTimer <= 0f)
        {
            TryStartReactiveEyeContact();

            // Only reset the decision timer here if we did NOT just engage.
            // While engaged, the timer is irrelevant until disengagement,
            // at which point CancelReactiveEyeContact() reseeds it.
            if (!reactiveEyeContactEngaged)
            {
                reactiveDecisionTimer = UnityEngine.Random.Range(
                    reactiveDecisionMinTime,
                    reactiveDecisionMaxTime);
            }
        }
    }

    private void TryStartReactiveEyeContact()
    {
        if (eyeContactTarget == null)
            return;

        if (!IsEyeContactTargetValid())
            return;

        float probability = GetReactiveEyeContactProbability();

        if (UnityEngine.Random.value <= probability)
        {
            reactiveEyeContactEngaged = true;

            // Reuse the existing Eye Contact cycle/timing entirely.
            ResetEyeContactState();
        }
    }

    private void CancelReactiveEyeContact()
    {
        bool wasEngaged = reactiveEyeContactEngaged;

        reactiveEyeContactEngaged = false;

        if (wasEngaged)
        {
            // Do not snap: let idleMovingPoint continue from wherever the
            // shared, already-smoothed look point currently is, and force
            // a fresh Idle target choice on the very next Idle tick. Any
            // jitter sequence in flight is left to finish naturally rather
            // than being cut off abruptly.
            idleMovingPoint = currentLookPoint;
            stateTimer = 0f;
        }

        reactiveDecisionTimer = UnityEngine.Random.Range(
            reactiveDecisionMinTime,
            reactiveDecisionMaxTime);
    }

    // =========================================================================
    // EYE CONTACT
    // =========================================================================

    private void UpdateEyeContact(float deltaTime)
    {
        if (!IsEyeContactTargetValid())
        {
            eyeContactJustEntered = false;

            if (reactiveEyeContactEngaged)
            {
                // Graceful cancellation: return to Idle without snapping.
                CancelReactiveEyeContact();
                desiredLookPoint = currentLookPoint;
                return;
            }

            // Graceful fallback:
            // If the conversational target is behind the character or too
            // far outside the configured FOV, simply return toward neutral.
            desiredLookPoint = GetForwardLookPoint();

            // Keep restarting the cycle so that once the target re-enters
            // the FOV the eye-contact behaviour begins cleanly.
            stateTimer = 0f;

            return;
        }

        // Look Away Time = 0 has explicit priority:
        // eyes remain on the conversational target indefinitely.
        if (lookAwayTime <= 0f && stareTime > 0f)
        {
            eyeContactLookingAway = false;

            Vector3 target = eyeContactTarget.position;

            SetLookTarget(
                target,
                eyeContactJustEntered ? target : (Vector3?)null,
                deltaTime);

            eyeContactJustEntered = false;
            return;
        }

        // Stare Time = 0 means never directly look at the target.
        if (stareTime <= 0f)
        {
            bool pickedNewAwayPoint = false;

            if (!eyeContactLookingAway || stateTimer <= 0f)
            {
                eyeContactLookingAway = true;
                PickEyeContactAwayPoint();
                pickedNewAwayPoint = true;

                // If Look Away Time is also zero there is no meaningful
                // cycle duration, so use a small refresh period.
                stateTimer = lookAwayTime > 0f
                    ? lookAwayTime
                    : 0.5f;
            }

            stateTimer -= deltaTime;

            bool isNew = pickedNewAwayPoint || eyeContactJustEntered;

            SetLookTarget(
                eyeContactAwayPoint,
                isNew ? eyeContactAwayPoint : (Vector3?)null,
                deltaTime);

            eyeContactJustEntered = false;
            return;
        }

        stateTimer -= deltaTime;

        bool justTransitioned = eyeContactJustEntered;
        eyeContactJustEntered = false;

        if (stateTimer <= 0f)
        {
            if (eyeContactLookingAway)
            {
                // Finished glancing away, returning to a direct stare.
                // This marks the completion of one full stare/look-away
                // cycle, which is the natural point for a purely reactive
                // engagement to disengage and hand control back to Idle.
                eyeContactLookingAway = false;
                stateTimer = GetStareDuration();
                justTransitioned = true;

                if (currentState == EyeState.Idle && reactiveEyeContactEngaged)
                {
                    CancelReactiveEyeContact();
                }
            }
            else
            {
                // Finished staring at the target.
                eyeContactLookingAway = true;
                PickEyeContactAwayPoint();
                stateTimer = lookAwayTime;
                justTransitioned = true;
            }
        }

        Vector3 continuousPoint = eyeContactLookingAway
            ? eyeContactAwayPoint
            : eyeContactTarget.position;

        SetLookTarget(
            continuousPoint,
            justTransitioned ? continuousPoint : (Vector3?)null,
            deltaTime);
    }

    private void PickEyeContactAwayPoint()
    {
        if (eyeContactTarget == null)
        {
            eyeContactAwayPoint = GetForwardLookPoint();
            return;
        }

        Vector3 eyeCenter = GetEyeCenter();

        Vector3 targetOffset =
            eyeContactTarget.position - eyeCenter;

        float targetDistance = targetOffset.magnitude;

        if (targetDistance < 0.001f)
        {
            eyeContactAwayPoint = GetForwardLookPoint();
            return;
        }

        Vector3 targetDirection =
            targetOffset / targetDistance;

        // Construct a basis centered on the ACTUAL direction to the
        // conversation target, rather than using a random full-scan direction.
        Vector3 characterUp = GetCharacterRestUp();

        Quaternion targetBasis =
            Quaternion.LookRotation(
                targetDirection,
                characterUp);

        float yaw = UnityEngine.Random.Range(
            -lookAwayHorizontalRange,
            lookAwayHorizontalRange);

        float pitch = UnityEngine.Random.Range(
            -lookAwayVerticalRange,
            lookAwayVerticalRange);

        Vector3 glanceDirection =
            targetBasis *
            Quaternion.Euler(-pitch, yaw, 0f) *
            Vector3.forward;

        eyeContactAwayPoint =
            eyeCenter +
            glanceDirection.normalized * targetDistance;
    }

    private float GetStareDuration()
    {
        if (stareTime <= 0f)
            return 0f;

        float min = Mathf.Min(
            minimumStareTime,
            stareTime);

        float baseDuration = stareTime;

        // randomness = 0 -> exactly Stare Time.
        // randomness = 1 -> anywhere from minimumStareTime to Stare Time.
        float randomDuration =
            UnityEngine.Random.Range(min, stareTime);

        return Mathf.Lerp(
            baseDuration,
            randomDuration,
            stareTimeRandomness);
    }

    private bool IsEyeContactTargetValid()
    {
        if (eyeContactTarget == null)
            return false;

        Vector3 center = GetEyeCenter();

        Vector3 toTarget =
            eyeContactTarget.position - center;

        if (toTarget.sqrMagnitude < 0.000001f)
            return true;

        toTarget.Normalize();

        Vector3 forward = GetCharacterRestForward();

        // Explicit behind-character rejection.
        if (Vector3.Dot(forward, toTarget) <= 0f)
            return false;

        float angle = Vector3.Angle(
            forward,
            toTarget);

        return angle <= eyeContactFieldOfView;
    }

    // =========================================================================
    // STARE
    // =========================================================================

    private void UpdateStare(float deltaTime)
    {
        if (eyeContactTarget != null)
        {
            /*
             * Stare deliberately does NOT apply the Eye Contact FOV rejection.
             *
             * It represents a hard lock onto the assigned target. The final
             * per-eye angular clamp still prevents physically impossible
             * rotations if that target moves behind the character.
             */
            Vector3 target = eyeContactTarget.position;

            SetLookTarget(
                target,
                stareJustEntered ? target : (Vector3?)null,
                deltaTime);
        }
        else
        {
            desiredLookPoint = GetForwardLookPoint();
        }

        stareJustEntered = false;
    }

    // =========================================================================
    // LOOK MOVEMENT / JITTER
    // =========================================================================

    /// <summary>
    /// Single choke point every state routes its computed look point through.
    /// If newDestination is non-null, this frame represents the start of a
    /// genuinely new point-to-point transition (a fresh "snap point"), and a
    /// jitter sequence toward it may be rolled. Otherwise continuousPoint is
    /// simply the state's live, frame-to-frame value (e.g. a moving target's
    /// current position, or Idle's own moving-point interpolation).
    /// </summary>
    private void SetLookTarget(Vector3 continuousPoint, Vector3? newDestination, float deltaTime)
    {
        if (newDestination.HasValue)
            BeginPossibleJitterTransition(newDestination.Value);

        desiredLookPoint = jitterActive
            ? AdvanceJitterTraversal(deltaTime)
            : continuousPoint;
    }

    /// <summary>
    /// Rolls Jitter Probability for a new destination and, on success,
    /// generates a short sequence of waypoints scattered around the direct
    /// line from the eyes' current position to that destination. The final
    /// waypoint is always the exact destination - jitter never introduces a
    /// permanent aiming error.
    /// </summary>
    private void BeginPossibleJitterTransition(Vector3 destination)
    {
        jitterActive = false;
        jitterWaypointCount = 0;
        jitterWaypointIndex = 0;
        jitterWaypointTimer = 0f;

        if (jitterProbability <= 0f)
            return;

        if (UnityEngine.Random.value > jitterProbability)
            return;

        Vector3 startPoint = currentLookPoint;
        float distance = Vector3.Distance(startPoint, destination);

        // Not enough travel distance for a staggered path to read as
        // anything other than noise.
        if (distance < MinJitterDistance)
            return;

        int scatterCount = Mathf.Clamp(
            Mathf.RoundToInt(jitterAmount * distance),
            MinJitterWaypoints,
            MaxJitterWaypoints);

        Vector3 direction = (destination - startPoint) / distance;

        // Build a stable perpendicular basis around the travel direction so
        // scatter offsets read as "off the line" rather than along it.
        Vector3 referenceUp = GetCharacterRestUp();
        Vector3 perpendicularA = Vector3.Cross(direction, referenceUp);

        if (perpendicularA.sqrMagnitude < 0.000001f)
            perpendicularA = Vector3.Cross(direction, Vector3.right);

        perpendicularA.Normalize();
        Vector3 perpendicularB = Vector3.Cross(direction, perpendicularA);

        float maxDeviation = distance * jitterRandomness * JitterDeviationScale;

        for (int i = 0; i < scatterCount; i++)
        {
            float t = (i + 1f) / (scatterCount + 1f);

            Vector3 pointOnLine = Vector3.Lerp(startPoint, destination, t);

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(0f, maxDeviation);

            Vector3 offset =
                (perpendicularA * Mathf.Cos(angle) + perpendicularB * Mathf.Sin(angle)) *
                radius;

            jitterWaypoints[i] = pointOnLine + offset;
        }

        // The final waypoint is always the real destination, exactly.
        jitterWaypoints[scatterCount] = destination;

        jitterWaypointCount = scatterCount + 1;
        jitterWaypointIndex = 0;
        jitterWaypointTimer = 0f;
        jitterActive = true;
        jitterJustCompleted = false;
    }

    /// <summary>
    /// Advances through the current jitter waypoint sequence once the eyes
    /// are close enough to the active waypoint (or a max-time safeguard
    /// trips so a slow-moving eye never stalls indefinitely). Returns the
    /// waypoint that should be fed to the shared SmoothDamp this frame.
    /// </summary>
    private Vector3 AdvanceJitterTraversal(float deltaTime)
    {
        Vector3 waypoint = jitterWaypoints[jitterWaypointIndex];

        float distanceToWaypoint = Vector3.Distance(currentLookPoint, waypoint);

        jitterWaypointTimer += deltaTime;

        bool arrived = distanceToWaypoint <= JitterWaypointArrivalTolerance;
        bool timedOut = jitterWaypointTimer >= JitterMaxWaypointTime;

        if (arrived || timedOut)
        {
            jitterWaypointIndex++;
            jitterWaypointTimer = 0f;

            if (jitterWaypointIndex >= jitterWaypointCount)
            {
                jitterActive = false;
                jitterJustCompleted = true;

                // Sequence complete - the last waypoint IS the real
                // destination, so just keep returning it.
                return jitterWaypoints[jitterWaypointCount - 1];
            }

            waypoint = jitterWaypoints[jitterWaypointIndex];
        }

        return waypoint;
    }

    // =========================================================================
    // LOOK POINT REFERENCE
    // =========================================================================

    /// <summary>
    /// If Look Point Reference is assigned, moves it to the eyes' actual
    /// gaze point (currentLookPoint) whenever that point's horizontal or
    /// vertical deviation from the character's straight-ahead direction
    /// meets or exceeds the configured trigger distance. When neither
    /// threshold is met, the reference object is deliberately left where
    /// it last was - this is what lets a downstream Multi-Aim Constraint
    /// ignore small idle scans and only react once the eyes look somewhere
    /// meaningfully off-center.
    /// </summary>
    private void UpdateLookPointReference()
    {
        if (lookPointReference == null)
            return;

        Vector3 center = GetEyeCenter();
        Vector3 forward = GetCharacterRestForward();
        Vector3 up = GetCharacterRestUp();
        Vector3 right = GetCharacterRestRight();

        Vector3 toLook = currentLookPoint - center;

        // Project the gaze point onto the straight-ahead ray so the lateral
        // offset is measured at the same depth as the gaze point itself,
        // rather than at some fixed procedural look distance.
        float forwardDistance = Vector3.Dot(toLook, forward);
        Vector3 pointOnForwardRay = center + forward * forwardDistance;

        Vector3 lateralOffset = currentLookPoint - pointOnForwardRay;

        float horizontalDeviation = Mathf.Abs(Vector3.Dot(lateralOffset, right));
        float verticalDeviation = Mathf.Abs(Vector3.Dot(lateralOffset, up));

        bool farEnough =
            horizontalDeviation >= lookPointHorizontalTriggerDistance ||
            verticalDeviation >= lookPointVerticalTriggerDistance;

        if (farEnough)
            lookPointReference.position = currentLookPoint;
    }

    // =========================================================================
    // STATE MANAGEMENT
    // =========================================================================

    private void ResetState()
    {
        stateTimer = 0f;

        switch (currentState)
        {
            case EyeState.Idle:
                ResetIdleState();
                break;

            case EyeState.EyeContact:
                ResetEyeContactState();
                break;

            case EyeState.Stare:
                stareJustEntered = true;
                break;
        }
    }

    private void ResetIdleState()
    {
        currentIdleInterestTarget = null;

        idleTargetPoint = GetForwardLookPoint();

        // Do not reset currentLookPoint. This preserves the cross-state blend.
        //
        // idleMovingPoint begins from the current shared point so entering Idle
        // doesn't create a second sudden discontinuity.
        idleMovingPoint = currentLookPoint;

        stateTimer = 0f;
    }

    private void ResetEyeContactState()
    {
        eyeContactJustEntered = true;

        if (stareTime <= 0f)
        {
            eyeContactLookingAway = true;
            PickEyeContactAwayPoint();

            stateTimer = lookAwayTime > 0f
                ? lookAwayTime
                : 0.5f;
        }
        else
        {
            eyeContactLookingAway = false;
            stateTimer = GetStareDuration();
        }
    }

    // =========================================================================
    // EYE ROTATION / CLAMPING
    // =========================================================================

    private void RotateEye(
        Transform eye,
        Quaternion restLocalRotation,
        Quaternion axisBasis)
    {
        if (eye == null)
            return;

        Vector3 toTarget =
            currentLookPoint - eye.position;

        if (toTarget.sqrMagnitude < 0.000001f)
            return;

        Vector3 desiredWorldDirection =
            toTarget.normalized;

        /*
         * Work out where the eye WOULD be if it were still in its calibrated
         * local rest rotation.
         *
         * This means head/neck/body animation is automatically inherited.
         * We are not storing a fixed world-space rest direction.
         */
        Quaternion restWorldRotation;

        if (eye.parent != null)
        {
            restWorldRotation =
                eye.parent.rotation *
                restLocalRotation;
        }
        else
        {
            restWorldRotation =
                restLocalRotation;
        }

        /*
         * axisBasis represents the selected eye-local Forward and Up axes.
         *
         * Multiplying the eye's calibrated transform rotation by this basis
         * gives us a conventional coordinate system where:
         *
         * +Z = calibrated eye forward
         * +Y = calibrated eye up
         */
        Quaternion restBasisWorld =
            restWorldRotation *
            axisBasis;

        // Convert desired target direction into the calibrated eye basis.
        Vector3 localDirection =
            Quaternion.Inverse(restBasisWorld) *
            desiredWorldDirection;

        float horizontalLength = Mathf.Sqrt(
            localDirection.x * localDirection.x +
            localDirection.z * localDirection.z);

        float yaw = Mathf.Atan2(
            localDirection.x,
            localDirection.z) * Mathf.Rad2Deg;

        float pitch = Mathf.Atan2(
            localDirection.y,
            horizontalLength) * Mathf.Rad2Deg;

        // Clamp independently from all state-specific behaviour.
        yaw = Mathf.Clamp(
            yaw,
            -horizontalClamp,
            horizontalClamp);

        pitch = Mathf.Clamp(
            pitch,
            -verticalClamp,
            verticalClamp);

        /*
         * Build the final clamped world basis directly from the calibrated
         * rest basis.
         *
         * Quaternion.Euler uses positive X rotation as downward pitch when
         * applied to Vector3.forward, hence -pitch here.
         */
        Quaternion clampedWorldBasis =
            restBasisWorld *
            Quaternion.Euler(-pitch, yaw, 0f);

        /*
         * Convert our conventional +Z/+Y basis back into this particular
         * eye object's selected local Forward/Up mapping.
         */
        eye.rotation =
            clampedWorldBasis *
            Quaternion.Inverse(axisBasis);
    }

    // =========================================================================
    // REST / CALIBRATION HELPERS
    // =========================================================================

    private Vector3 GetEyeCenter()
    {
        if (leftEye != null && rightEye != null)
            return (leftEye.position + rightEye.position) * 0.5f;

        if (leftEye != null)
            return leftEye.position;

        if (rightEye != null)
            return rightEye.position;

        return transform.position;
    }

    private Quaternion GetCharacterRestBasis()
    {
        Vector3 forward = GetCharacterRestForward();
        Vector3 up = GetCharacterRestUp();

        // Quaternion is a value type, so this does not generate GC garbage.
        return Quaternion.LookRotation(forward, up);
    }

    private Vector3 GetCharacterRestForward()
    {
        Vector3 leftForward =
            GetRestAxisWorld(
                leftEye,
                leftRestLocalRotation,
                leftForwardAxis);

        Vector3 rightForward =
            GetRestAxisWorld(
                rightEye,
                rightRestLocalRotation,
                rightForwardAxis);

        Vector3 average =
            leftForward + rightForward;

        if (average.sqrMagnitude < 0.000001f)
            return transform.forward;

        return average.normalized;
    }

    private Vector3 GetCharacterRestUp()
    {
        Vector3 leftUp =
            GetRestAxisWorld(
                leftEye,
                leftRestLocalRotation,
                leftUpAxis);

        Vector3 rightUp =
            GetRestAxisWorld(
                rightEye,
                rightRestLocalRotation,
                rightUpAxis);

        Vector3 average =
            leftUp + rightUp;

        if (average.sqrMagnitude < 0.000001f)
            return transform.up;

        return average.normalized;
    }

    private Vector3 GetCharacterRestRight()
    {
        Vector3 forward = GetCharacterRestForward();
        Vector3 up = GetCharacterRestUp();

        Vector3 right = Vector3.Cross(up, forward);

        if (right.sqrMagnitude < 0.000001f)
            return transform.right;

        return right.normalized;
    }

    private Vector3 GetRestAxisWorld(
        Transform eye,
        Quaternion restLocalRotation,
        LocalAxis axis)
    {
        if (eye == null)
            return transform.forward;

        Quaternion restWorldRotation;

        if (eye.parent != null)
        {
            restWorldRotation =
                eye.parent.rotation *
                restLocalRotation;
        }
        else
        {
            restWorldRotation =
                restLocalRotation;
        }

        return restWorldRotation * AxisToVector(axis);
    }

    private Vector3 GetForwardLookPoint()
    {
        return
            GetEyeCenter() +
            GetCharacterRestForward() *
            proceduralLookDistance;
    }

    // =========================================================================
    // AXIS MAPPING
    // =========================================================================

    private static Vector3 AxisToVector(LocalAxis axis)
    {
        switch (axis)
        {
            case LocalAxis.PositiveX:
                return Vector3.right;

            case LocalAxis.NegativeX:
                return Vector3.left;

            case LocalAxis.PositiveY:
                return Vector3.up;

            case LocalAxis.NegativeY:
                return Vector3.down;

            case LocalAxis.PositiveZ:
                return Vector3.forward;

            case LocalAxis.NegativeZ:
                return Vector3.back;

            default:
                return Vector3.forward;
        }
    }

    private static Quaternion CreateAxisBasis(
        LocalAxis forwardAxis,
        LocalAxis upAxis)
    {
        Vector3 forward = AxisToVector(forwardAxis);
        Vector3 up = AxisToVector(upAxis);

        if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
        {
            // Invalid configuration. Forward and Up cannot point along
            // the same/opposite axis.
            up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) < 0.99f
                ? Vector3.up
                : Vector3.right;
        }

        return Quaternion.LookRotation(
            forward,
            up);
    }

    private void ValidateAxes()
    {
        ValidateEyeAxes(
            "Left Eye",
            leftForwardAxis,
            leftUpAxis);

        ValidateEyeAxes(
            "Right Eye",
            rightForwardAxis,
            rightUpAxis);
    }

    private void ValidateEyeAxes(
        string eyeName,
        LocalAxis forward,
        LocalAxis up)
    {
        float dot = Mathf.Abs(
            Vector3.Dot(
                AxisToVector(forward),
                AxisToVector(up)));

        if (dot > 0.99f)
        {
            Debug.LogWarning(
                $"{eyeName}: Forward and Up axes cannot be parallel. " +
                "Choose two different perpendicular axes.",
                this);
        }
    }

    // =========================================================================
    // GIZMOS
    // =========================================================================

    private void OnDrawGizmosSelected()
    {
        DrawEyeAxisGizmo(
            leftEye,
            leftForwardAxis,
            leftUpAxis);

        DrawEyeAxisGizmo(
            rightEye,
            rightForwardAxis,
            rightUpAxis);

        // Interest Points: drawn even outside Play Mode so they can be
        // authored/placed with immediate visual feedback.
        if (interestPoints != null && (leftEye != null || rightEye != null))
        {
            Vector3 center = GetEyeCenter();

            for (int i = 0; i < interestPoints.Count; i++)
            {
                InterestPoint point = interestPoints[i];

                if (point == null || point.target == null)
                    continue;

                bool isCurrent =
                    Application.isPlaying &&
                    point.target == currentIdleInterestTarget;

                Gizmos.color = isCurrent ? Color.green : Color.yellow;

                Gizmos.DrawWireSphere(point.target.position, 0.05f);
                Gizmos.DrawLine(center, point.target.position);
            }
        }

        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(
            currentLookPoint,
            0.025f);

        if (leftEye != null)
        {
            Gizmos.DrawLine(
                leftEye.position,
                currentLookPoint);
        }

        if (rightEye != null)
        {
            Gizmos.DrawLine(
                rightEye.position,
                currentLookPoint);
        }

        // Active jitter waypoints, so the staggered path can be inspected.
        if (jitterActive && jitterWaypointCount > 0)
        {
            Gizmos.color = Color.magenta;

            for (int i = 0; i < jitterWaypointCount; i++)
            {
                Gizmos.DrawWireSphere(jitterWaypoints[i], 0.015f);

                if (i > 0)
                    Gizmos.DrawLine(jitterWaypoints[i - 1], jitterWaypoints[i]);
            }
        }

        if (reactiveEyeContact && eyeContactTarget != null)
        {
            Gizmos.color = reactiveEyeContactEngaged
                ? Color.cyan
                : new Color(0f, 1f, 1f, 0.3f);

            Gizmos.DrawLine(
                GetEyeCenter(),
                eyeContactTarget.position);
        }
    }

    private static void DrawEyeAxisGizmo(
        Transform eye,
        LocalAxis forwardAxis,
        LocalAxis upAxis)
    {
        if (eye == null)
            return;

        const float axisLength = 0.08f;

        Vector3 forward =
            eye.TransformDirection(
                AxisToVector(forwardAxis));

        Vector3 up =
            eye.TransformDirection(
                AxisToVector(upAxis));

        Gizmos.DrawLine(
            eye.position,
            eye.position + forward * axisLength);

        Gizmos.DrawWireSphere(
            eye.position + forward * axisLength,
            0.005f);

        Gizmos.DrawLine(
            eye.position,
            eye.position + up * axisLength);
    }
}