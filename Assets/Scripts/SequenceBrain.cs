using System;
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

    [SerializeField] private GameObject timeline;
    [SerializeField] private PlayableDirector director;

    [Header("Systems toggled by state")]
    [Tooltip("Enable these while IN SEQUENCE (e.g., reticle off, grab off, etc.).")]
    [SerializeField] private Behaviour[] enableInSequence;

    [Tooltip("Enable these while IN INTERACTION (e.g., DistanceHandGrabInteractor, UI ray, etc.).")]
    [SerializeField] private Behaviour[] enableInInteraction;

    [Header("Optional: modules you want globally toggled")]
    [Tooltip("Modular interaction behaviours you’ll enable only during interaction. Leave empty for now.")]
    [SerializeField] private Behaviour[] interactionModules;

    public SequenceState State { get; private set; } = SequenceState.InSequence;

    private void Reset()
    {
        timeline.GetComponent<PlayableDirector>();
        //director = GetComponent<PlayableDirector>();
    }

    private void Awake()
    {
        if (!director)
            director = timeline.GetComponent<PlayableDirector>();
            //director = GetComponent<PlayableDirector>();

        ApplyState(SequenceState.InSequence, pauseDirector: false);
    }



    // Called by a Timeline Signal (end of audio / gate point)
    public void EnterInteraction()
    {
        if (State == SequenceState.InInteraction || State == SequenceState.TransitionToInteraction)
            return;

        ApplyState(SequenceState.TransitionToInteraction, pauseDirector: true);
        ApplyState(SequenceState.InInteraction, pauseDirector: true);
    }

    // Called when interaction is complete (you’ll wire this later)
    public void ExitInteractionResumeTimeline()
    {
        if (State == SequenceState.InSequence || State == SequenceState.TransitionToSequence)
            return;

        ApplyState(SequenceState.TransitionToSequence, pauseDirector: true);
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

        // Interaction modules are only active in interaction (baseline).
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
}
