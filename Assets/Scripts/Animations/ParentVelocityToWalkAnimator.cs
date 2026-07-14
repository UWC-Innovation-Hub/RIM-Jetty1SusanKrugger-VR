using UnityEngine;

public class ParentVelocityToWalkAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator prisonerAnimator;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string walkPlaybackParameterName = "WalkPlayback";

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

    private Vector3 previousPosition;
    private int speedParameter;
    private int walkPlaybackParameter;

    private void Awake()
    {
        speedParameter = Animator.StringToHash(speedParameterName);
        walkPlaybackParameter = Animator.StringToHash(walkPlaybackParameterName);
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (prisonerAnimator == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - previousPosition;
        previousPosition = currentPosition;

        // Ignore vertical motion. We only care about ground-plane travel speed.
        delta.y = 0f;

        float horizontalSpeed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        float animatorSpeed = horizontalSpeed < idleThreshold ? 0f : horizontalSpeed;

        float playback = authoredWalkSpeed > 0f
            ? horizontalSpeed / authoredWalkSpeed
            : 1f;

        playback = Mathf.Clamp(playback, minWalkPlayback, maxWalkPlayback);

        prisonerAnimator.SetFloat(speedParameter, animatorSpeed, speedDampTime, Time.deltaTime);
        prisonerAnimator.SetFloat(walkPlaybackParameter, playback, speedDampTime, Time.deltaTime);
    }
}