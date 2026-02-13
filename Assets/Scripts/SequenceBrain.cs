using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class SequenceBrain : MonoBehaviour
{
    public enum SequenceState
    {
        InSequence,
        TransitionToInteraction,
        InInteraction,
        TransitionToSequence
    }

    [Header("Wiring")]
    [SerializeField] private GameObject timeline;              // GO that holds PlayableDirector
    [SerializeField] private PlayableDirector director;

    [Header("Active Interaction (event-driven)")]
    [Tooltip("The interaction module to activate when entering InInteraction. Set via the signal router.")]
    [SerializeField] private InteractionModuleBase activeInteraction;

    [Header("Systems toggled by state")]
    [SerializeField] private Behaviour[] enableInSequence;
    [FormerlySerializedAs("enableInInteraction")]
    [SerializeField] private Behaviour[] enableInInteractionShared;

    [Header("Optional: extra shared interaction-only behaviours")]
    [FormerlySerializedAs("interactionModules")]
    [SerializeField] private Behaviour[] interactionSharedExtras;

    [Header("Startup enforcement")]
    [SerializeField] private bool enforceStartupStateAfterFirstFrame = true;

    public SequenceState State { get; private set; } = SequenceState.InSequence;

    private Coroutine _startupEnforceRoutine;

    private void Reset()
    {
        if (!timeline) timeline = gameObject;
        director = timeline ? timeline.GetComponent<PlayableDirector>() : GetComponent<PlayableDirector>();
    }

    private void Awake()
    {
        if (!director)
            director = timeline ? timeline.GetComponent<PlayableDirector>() : GetComponent<PlayableDirector>();

        ApplyState(SequenceState.InSequence, pauseDirector: false);
    }

    private void Start()
    {
        if (enforceStartupStateAfterFirstFrame)
        {
            if (_startupEnforceRoutine != null) StopCoroutine(_startupEnforceRoutine);
            _startupEnforceRoutine = StartCoroutine(EnforceInitialStateNextFrame());
        }
    }

    //Possibly invalid code; what is the reason for this really? Previous issue with interaction on start up?
    //maybe better just to move the sequence signal slightly down the line.
    private IEnumerator EnforceInitialStateNextFrame()
    {
        yield return null; // wait 1 frame (OVR/Meta init tends to happen around here)
        ApplyState(SequenceState.InSequence, pauseDirector: false);
        _startupEnforceRoutine = null;
    }

    /// Call this BEFORE EnterInteraction() to choose which interaction module is active.
    public void SetActiveInteraction(InteractionModuleBase module)
    {
        if (activeInteraction == module) return;

        // If we were listening to a previous module, detach.
        if (activeInteraction != null)
            activeInteraction.Completed -= OnActiveInteractionCompleted;

        activeInteraction = module;
    }

    /// Called by Timeline signal router at a gate moment.
    public void EnterInteraction()
    {
        if (State == SequenceState.InInteraction || State == SequenceState.TransitionToInteraction)
            return;

        ApplyState(SequenceState.TransitionToInteraction, pauseDirector: true);

        if (activeInteraction)
        {
            // Defensive: prevent double subscribe
            activeInteraction.Completed -= OnActiveInteractionCompleted;

            activeInteraction.Activate();
            activeInteraction.Completed += OnActiveInteractionCompleted;
        }
        else
        {
            Debug.LogWarning($"{name}: EnterInteraction() called but no activeInteraction is assigned.");
        }

        ApplyState(SequenceState.InInteraction, pauseDirector: true);
    }

    private void OnActiveInteractionCompleted()
    {
        if (activeInteraction != null)
            activeInteraction.Completed -= OnActiveInteractionCompleted;

        ExitInteractionResumeTimeline();
    }

    public void ExitInteractionResumeTimeline()
    {
        if (State == SequenceState.InSequence || State == SequenceState.TransitionToSequence)
            return;

        ApplyState(SequenceState.TransitionToSequence, pauseDirector: true);

        if (activeInteraction != null)
            activeInteraction.Deactivate();

        ApplyState(SequenceState.InSequence, pauseDirector: false);
    }

    private void ApplyState(SequenceState newState, bool pauseDirector)
    {
        State = newState;

        // Director control
        if (director)
        {
            if (pauseDirector)
            {
                if (director.state == PlayState.Playing)
                    director.Pause();
            }
            else
            {
                if (director.state != PlayState.Playing)
                    director.Play();
            }
        }

        // Toggle systems
        bool inSeq = (newState == SequenceState.InSequence);
        bool inInt = (newState == SequenceState.InInteraction);

        SetEnabled(enableInSequence, inSeq);
        SetEnabled(enableInInteractionShared, inInt);
        SetEnabled(interactionSharedExtras, inInt);
    }

    private static void SetEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null) return;
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (!behaviours[i]) continue;
            behaviours[i].enabled = enabled;
        }
    }
}
