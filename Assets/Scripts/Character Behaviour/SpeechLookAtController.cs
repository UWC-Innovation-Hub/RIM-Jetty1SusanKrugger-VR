using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Drives a Head and a Spine Multi-Aim Constraint (Animation Rigging package)
/// so the character looks toward whatever those constraints are targeting
/// (typically the player) while speaking.
///
/// Toggle IsSpeaking on when the character starts talking - the constraint
/// weights smoothly blend up to their configured max weight. Toggle it off
/// when they stop talking and the weights smoothly blend back down to 0.
///
/// The MultiAimConstraint's own source objects (e.g. the player Transform)
/// should already be configured on the constraint in the Rig Builder setup;
/// this script only drives how much influence ("weight") each constraint has
/// over time.
/// </summary>
[DisallowMultipleComponent]
public class SpeechLookAtController : MonoBehaviour
{
    [Header("Speaking State")]
    [Tooltip(
        "Toggle this on while the character is talking. The look-at weight " +
        "smoothly blends in; turning it off smoothly blends the weight back to 0.")]
    [SerializeField] private bool isSpeaking = false;

    [Header("Blend Settings")]
    [Tooltip(
        "How fast the constraint weight moves toward its target value, in " +
        "weight units per second. Higher = snappier transition, lower = " +
        "slower/smoother transition. Applies to both the Head and Spine constraints.")]
    [Min(0.01f)]
    public float transitionSpeed = 3f;

    [Header("Head Constraint")]
    [Tooltip("The Multi-Aim Constraint driving the head bone.")]
    [SerializeField] private MultiAimConstraint headConstraint;

    [Tooltip("Maximum weight the Head constraint blends up to while speaking.")]
    [Range(0f, 1f)]
    [SerializeField] private float headMaxWeight = 1f;

    [Header("Spine Constraint")]
    [Tooltip("The Multi-Aim Constraint driving the spine bone.")]
    [SerializeField] private MultiAimConstraint spineConstraint;

    [Tooltip("Maximum weight the Spine constraint blends up to while speaking.")]
    [Range(0f, 1f)]
    [SerializeField] private float spineMaxWeight = 1f;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Current speaking state. Setting this does not snap the constraints -
    /// they smoothly blend toward their new target weight over time,
    /// governed by transitionSpeed.
    /// </summary>
    public bool IsSpeaking
    {
        get => isSpeaking;
        set => isSpeaking = value;
    }

    /// <summary>Convenience call: begins blending both constraints in.</summary>
    public void StartSpeaking()
    {
        isSpeaking = true;
    }

    /// <summary>Convenience call: begins blending both constraints out.</summary>
    public void StopSpeaking()
    {
        isSpeaking = false;
    }

    /// <summary>Current live weight of the Head constraint, or 0 if unassigned.</summary>
    public float CurrentHeadWeight => headConstraint != null ? headConstraint.weight : 0f;

    /// <summary>Current live weight of the Spine constraint, or 0 if unassigned.</summary>
    public float CurrentSpineWeight => spineConstraint != null ? spineConstraint.weight : 0f;

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateConstraintWeight(headConstraint, headMaxWeight, deltaTime);
        UpdateConstraintWeight(spineConstraint, spineMaxWeight, deltaTime);
    }

    private void UpdateConstraintWeight(
        MultiAimConstraint constraint,
        float maxWeight,
        float deltaTime)
    {
        if (constraint == null)
            return;

        float targetWeight = isSpeaking ? maxWeight : 0f;

        constraint.weight = Mathf.MoveTowards(
            constraint.weight,
            targetWeight,
            transitionSpeed * deltaTime);
    }
}