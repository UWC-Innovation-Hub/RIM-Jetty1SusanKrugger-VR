using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

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
    [SerializeField] private GameObject timeline;              // Your timeline GO that holds the PlayableDirector
    [SerializeField] private PlayableDirector director;

    [Header("Active Interaction (event-driven)")]
    [Tooltip("The interaction module that will be activated when EnterInteraction() is called. " +
             "It must inherit from InteractionModuleBase (e.g., PrisonerSortModule).")]
    [SerializeField] private InteractionModuleBase activeInteraction;

    [Header("Systems toggled by state")]
    [Tooltip("Enable these while IN SEQUENCE (e.g., reticle off, grab off, etc.).")]
    [SerializeField] private Behaviour[] enableInSequence;

    [Tooltip("Enable these while IN INTERACTION (e.g., DistanceHandGrabInteractor, UI ray, etc.).")]
    [SerializeField] private Behaviour[] enableInInteraction;

    [Header("Optional: modules you want globally toggled")]
    [Tooltip("Additional behaviours you want enabled only during interaction (prompts, UI roots, highlights, etc.).")]
    [SerializeField] private Behaviour[] interactionModules;

    [Header("Startup enforcement")]
    [Tooltip("Re-apply the initial state after a frame to override SDK init that may re-enable components.")]
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
        // Some Meta/OVR init paths re-enable interactors in Start/first frame.
        // Re-apply our desired state after everything has bootstrapped.
        if (enforceStartupStateAfterFirstFrame)
        {
            if (_startupEnforceRoutine != null) StopCoroutine(_startupEnforceRoutine);
            _startupEnforceRoutine = StartCoroutine(EnforceInitialStateNextFrame());
        }
    }

    private IEnumerator EnforceInitialStateNextFrame()
    {
        yield return null; // wait 1 frame
        ApplyState(SequenceState.InSequence, pauseDirector: false);
        _startupEnforceRoutine = null;
    }

    // Called by a Timeline Signal (end of audio / gate point)
    public void EnterInteraction()
    {
        if (State == SequenceState.InInteraction || State == SequenceState.TransitionToInteraction)
            return;

        ApplyState(SequenceState.TransitionToInteraction, pauseDirector: true);

        // Activate + subscribe to completion
        if (activeInteraction)
        {
            // Defensive: ensure we don't double-subscribe
            activeInteraction.Completed -= OnActiveInteractionCompleted;

            activeInteraction.Activate();
            activeInteraction.Completed += OnActiveInteractionCompleted;
        }
        else
        {
            Debug.LogWarning($"{name}: EnterInteraction() called but no Active Interaction is assigned.");
        }

        ApplyState(SequenceState.InInteraction, pauseDirector: true);
    }

    // Event handler for the current interaction module
    private void OnActiveInteractionCompleted()
    {
        // Unsubscribe immediately to avoid any double-fire edge cases
        if (activeInteraction)
            activeInteraction.Completed -= OnActiveInteractionCompleted;

        ExitInteractionResumeTimeline();
    }

    // Called when interaction is complete (now driven by the module event)
    public void ExitInteractionResumeTimeline()
    {
        if (State == SequenceState.InSequence || State == SequenceState.TransitionToSequence)
            return;

        ApplyState(SequenceState.TransitionToSequence, pauseDirector: true);

        // Deactivate current interaction module
        if (activeInteraction)
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
                // Pause keeps timeline time at current frame and stops evaluation.
                // This does NOT affect regular animators/physics/audio outside timeline.
                if (director.state == PlayState.Playing)
                    director.Pause();
            }
            else
            {
                // Resume timeline.
                if (director.state != PlayState.Playing)
                    director.Play();
            }
        }

        // Toggle systems
        bool inSeq = (newState == SequenceState.InSequence);
        bool inInt = (newState == SequenceState.InInteraction);

        SetEnabled(enableInSequence, inSeq);
        SetEnabled(enableInInteraction, inInt);

        // Optional extra interaction-only behaviours
        SetEnabled(interactionModules, inInt);
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

    // Optional helper if you want to switch interactions from elsewhere (e.g., Timeline signal router)
    public void SetActiveInteraction(InteractionModuleBase module)
    {
        if (activeInteraction == module) return;

        // If we're currently listening to an old module, detach
        if (activeInteraction != null)
            activeInteraction.Completed -= OnActiveInteractionCompleted;

        activeInteraction = module;
    }
}
