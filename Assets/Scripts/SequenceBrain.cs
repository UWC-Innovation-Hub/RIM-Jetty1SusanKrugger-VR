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
    [SerializeField] private FadeController fadeController;

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
    [SerializeField] private bool fadeOnStartup = false;
    [SerializeField] private float startupFadeInDuration = 0.5f;

    [Header("Director Refresh")]
    [SerializeField] private bool evaluateDirectorOnPauseResume = true;

    public SequenceState State { get; private set; } = SequenceState.InSequence;

    private Coroutine _startupEnforceRoutine;
    private Coroutine _transitionRoutine;
    private bool _isTransitioning;
    private bool _pendingExitAfterTransition;

    private void Reset()
    {
        if (!timeline) timeline = gameObject;
        director = timeline ? timeline.GetComponent<PlayableDirector>() : GetComponent<PlayableDirector>();
        if (!fadeController) fadeController = FindFirstObjectByType<FadeController>();
    }

    private void Awake()
    {
        if (!director)
            director = timeline ? timeline.GetComponent<PlayableDirector>() : GetComponent<PlayableDirector>();

        if (!fadeController)
            fadeController = FindFirstObjectByType<FadeController>();

        if (fadeOnStartup && fadeController != null)
            fadeController.SetBlackInstant();

        ApplyState(SequenceState.InSequence, pauseDirector: false);
    }

    private void Start()
    {
        if (_startupEnforceRoutine != null) StopCoroutine(_startupEnforceRoutine);
        _startupEnforceRoutine = StartCoroutine(StartupSequenceRoutine());
    }

    private IEnumerator StartupSequenceRoutine()
    {
        if (enforceStartupStateAfterFirstFrame)
            yield return null; // wait 1 frame (OVR/Meta init tends to happen around here)

        ApplyState(SequenceState.InSequence, pauseDirector: false);

        if (fadeOnStartup && fadeController != null)
            yield return fadeController.FadeInRoutine(startupFadeInDuration);

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
        if (_isTransitioning)
            return;

        if (State == SequenceState.InInteraction || State == SequenceState.TransitionToInteraction)
            return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _pendingExitAfterTransition = false;
        _transitionRoutine = StartCoroutine(EnterInteractionRoutine());
    }

    private IEnumerator EnterInteractionRoutine()
    {
        _isTransitioning = true;
        ApplyState(SequenceState.TransitionToInteraction, pauseDirector: true);

        if (activeInteraction)
        {
            // Defensive: prevent double subscribe
            activeInteraction.Completed -= OnActiveInteractionCompleted;

            if (ShouldUseFade(activeInteraction))
                yield return fadeController.FadeOutRoutine(activeInteraction.FadeOutDuration);

            activeInteraction.Activate();
            activeInteraction.Completed += OnActiveInteractionCompleted;

            ApplyState(SequenceState.InInteraction, pauseDirector: true);

            if (ShouldUseFade(activeInteraction))
                yield return fadeController.FadeInRoutine(activeInteraction.FadeInDuration);
        }
        else
        {
            Debug.LogWarning($"{name}: EnterInteraction() called but no activeInteraction is assigned.");
            ApplyState(SequenceState.InInteraction, pauseDirector: true);
        }

        _isTransitioning = false;
        _transitionRoutine = null;

        if (_pendingExitAfterTransition)
        {
            _pendingExitAfterTransition = false;
            ExitInteractionResumeTimeline();
        }
    }

    private void OnActiveInteractionCompleted()
    {
        if (activeInteraction != null)
            activeInteraction.Completed -= OnActiveInteractionCompleted;

        ExitInteractionResumeTimeline();
    }

    public void ExitInteractionResumeTimeline()
    {
        if (_isTransitioning)
        {
            _pendingExitAfterTransition = true;
            return;
        }

        if (State == SequenceState.InSequence || State == SequenceState.TransitionToSequence)
            return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(ExitInteractionRoutine());
    }

    private IEnumerator ExitInteractionRoutine()
    {
        _isTransitioning = true;
        _pendingExitAfterTransition = false;

        ApplyState(SequenceState.TransitionToSequence, pauseDirector: true);

        InteractionModuleBase module = activeInteraction;

        if (module != null && ShouldUseFade(module))
            yield return fadeController.FadeOutRoutine(module.FadeOutDuration);

        if (module != null)
            module.Deactivate();

        ApplyState(SequenceState.InSequence, pauseDirector: false);

        if (module != null && ShouldUseFade(module))
            yield return fadeController.FadeInRoutine(module.FadeInDuration);

        _isTransitioning = false;
        _transitionRoutine = null;
    }

    private bool ShouldUseFade(InteractionModuleBase module)
    {
        return module != null && module.UseTransitionFade && fadeController != null;
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
                {
                    director.Pause();
                }

                if (evaluateDirectorOnPauseResume)
                {
                    director.Evaluate();
                }
            }
            else
            {
                if (director.state != PlayState.Playing)
                {
                    director.Play();
                }

                if (evaluateDirectorOnPauseResume)
                {
                    director.Evaluate();
                }
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
