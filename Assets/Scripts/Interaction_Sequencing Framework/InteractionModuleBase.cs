using System;
using UnityEngine;
using UnityEngine.Playables;

public abstract class InteractionModuleBase : MonoBehaviour
{
    /// Fired once when this interaction is considered complete for the current activation.
    public event Action Completed;

    public bool IsActive { get; private set; }
    public bool IsComplete { get; private set; }

    [Header("Module-local activation toggles")]
    [Tooltip("Enabled while this module is active; disabled when inactive.")]
    [SerializeField] private Behaviour[] enableWhenActive;
    [Tooltip("Disabled while this module is active; enabled when inactive.")]
    [SerializeField] private Behaviour[] disableWhenActive;
    [Tooltip("Set active while this module is active; set inactive when inactive.")]
    [SerializeField] private GameObject[] activeWhenActive;
    [Tooltip("Set inactive while this module is active; set active when inactive.")]
    [SerializeField] private GameObject[] inactiveWhenActive;
    [Tooltip("Force this module's local toggles OFF on scene load until Activate() is called.")]
    [SerializeField] private bool forceInactiveOnAwake = true;

    [Header("Optional Interaction Timeline")]
    [Tooltip("Optional interaction-specific PlayableDirector to play while this module is active.")]
    [SerializeField] private PlayableDirector interactionTimelineDirector;
    [Tooltip("Automatically find a child PlayableDirector when no explicit interaction timeline is assigned.")]
    [SerializeField] private bool autoFindInteractionTimeline = true;
    [Tooltip("Play the interaction timeline when Activate() is called.")]
    [SerializeField] private bool playInteractionTimelineOnActivate = true;
    [Tooltip("Stop and rewind the interaction timeline when Deactivate() is called.")]
    [SerializeField] private bool stopInteractionTimelineOnDeactivate = true;
    [Tooltip("Rewind the interaction timeline to the beginning before playing it on Activate().")]
    [SerializeField] private bool rewindInteractionTimelineOnActivate = true;

    [Header("Optional Tutorial Video")]
    [Tooltip("Optional tutorial video controller that preloads the tutorial clip for this interaction before its timeline starts.")]
    [SerializeField] private TutorialVideoController tutorialVideoController;
    [Tooltip("Tutorial clip index to preload on Activate(). Use -1 to skip tutorial preload for this interaction.")]
    [SerializeField] private int tutorialClipIndex = -1;
    [Tooltip("Stop the tutorial video when Deactivate() is called.")]
    [SerializeField] private bool stopTutorialVideoOnDeactivate = true;

    [Header("Optional Interaction Timeout")]
    [Tooltip("When enabled, automatically complete this interaction if the player does not finish it before the timeout threshold.")]
    [SerializeField] private bool enableInteractionTimeout = false;
    [Tooltip("How long this interaction can remain active before it auto-completes.")]
    [Min(0f)]
    [SerializeField] private float interactionTimeoutSeconds = 30f;

    [Header("Optional Environment")]
    [Tooltip("When active, force RenderSettings.fog to this value.")]
    [SerializeField] private bool enableFog = false;
    [Tooltip("Restore previous RenderSettings.fog value when this module deactivates.")]
    [SerializeField] private bool restoreFogOnDeactivate = true;

    [Header("Optional Transition Fade")]
    [SerializeField] private bool useTransitionFade = false;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [Tooltip("When ticked, SequenceBrain will NOT fade out before activating this interaction. " +
             "Use this when the sequence Timeline already reaches black before the interaction gate.")]
    [SerializeField] private bool skipEnterFadeOut = false;
    [Tooltip("When ticked, SequenceBrain will NOT fade in after activating this interaction. " +
             "Use this for the opening inventory interaction where the Timeline controls the fade itself.")]
    [SerializeField] private bool skipEnterFadeIn = false;
    [Tooltip("When ticked, SequenceBrain will NOT fade in after this interaction completes. " +
             "Use this when the resumed sequence Timeline owns the return fade-in.")]
    [SerializeField] private bool skipExitFadeIn = false;

    private bool _previousFogState;
    private bool _hasFogStateSnapshot;
    private Coroutine _interactionTimeoutRoutine;

    public bool UseTransitionFade => useTransitionFade;
    public float FadeOutDuration => fadeOutDuration;
    public float FadeInDuration => fadeInDuration;
    public bool SkipEnterFadeOut => skipEnterFadeOut;
    public bool SkipEnterFadeIn => skipEnterFadeIn;
    public bool SkipExitFadeIn => skipExitFadeIn;

    protected virtual void Awake()
    {
        IsActive = false;
        IsComplete = false;
        ResetInteractionTimelineToStart();

        if (forceInactiveOnAwake)
        {
            SetEnabled(enableWhenActive, false);
            SetEnabled(disableWhenActive, true);
            SetActive(activeWhenActive, false);
            SetActive(inactiveWhenActive, true);
        }
    }

    public virtual void Activate()
    {
        IsActive = true;
        IsComplete = false;
        _previousFogState = RenderSettings.fog;
        _hasFogStateSnapshot = true;

        SetEnabled(enableWhenActive, true);
        SetEnabled(disableWhenActive, false);
        SetActive(activeWhenActive, true);
        SetActive(inactiveWhenActive, false);
        RenderSettings.fog = enableFog;
        PreloadTutorialVideo();
        BeginInteractionTimeoutIfNeeded();

        if (playInteractionTimelineOnActivate)
        {
            PlayInteractionTimeline();
        }
    }

    public virtual void Deactivate()
    {
        StopInteractionTimeout();

        if (stopTutorialVideoOnDeactivate && tutorialVideoController != null)
        {
            tutorialVideoController.StopTutorial();
        }

        if (stopInteractionTimelineOnDeactivate)
        {
            ResetInteractionTimelineToStart();
        }

        IsActive = false;
        SetEnabled(enableWhenActive, false);
        SetEnabled(disableWhenActive, true);
        SetActive(activeWhenActive, false);
        SetActive(inactiveWhenActive, true);

        if (restoreFogOnDeactivate && _hasFogStateSnapshot)
        {
            RenderSettings.fog = _previousFogState;
        }
    }

    private void PlayInteractionTimeline()
    {
        PlayableDirector director = ResolveInteractionTimelineDirector();
        if (director == null)
        {
            return;
        }

        if (rewindInteractionTimelineOnActivate)
        {
            director.time = 0d;
            director.Evaluate();
        }

        director.Play();
    }

    private void ResetInteractionTimelineToStart()
    {
        PlayableDirector director = ResolveInteractionTimelineDirector();
        if (director == null)
        {
            return;
        }

        director.Stop();
        director.time = 0d;
        director.Evaluate();
    }

    /// <summary>
    /// Releases Timeline's animation graph before a script-driven exit fade.
    /// Deliberately does not rewind or evaluate frame zero, since that could
    /// overwrite the fader before SequenceBrain restores its captured alpha.
    /// </summary>
    public void ReleaseInteractionTimelineForExitFade()
    {
        PlayableDirector director = ResolveInteractionTimelineDirector();
        if (director == null)
        {
            return;
        }

        director.Stop();
    }

    private PlayableDirector ResolveInteractionTimelineDirector()
    {
        if (interactionTimelineDirector != null)
        {
            return interactionTimelineDirector;
        }

        if (!autoFindInteractionTimeline)
        {
            return null;
        }

        interactionTimelineDirector = GetComponentInChildren<PlayableDirector>(true);
        return interactionTimelineDirector;
    }

    private void PreloadTutorialVideo()
    {
        if (tutorialVideoController == null || tutorialClipIndex < 0)
        {
            return;
        }

        tutorialVideoController.SelectTutorialClip(tutorialClipIndex);
    }

    protected void Complete()
    {
        if (!IsActive || IsComplete) return;

        StopInteractionTimeout();
        IsComplete = true;
        Completed?.Invoke();
    }

    private void BeginInteractionTimeoutIfNeeded()
    {
        StopInteractionTimeout();

        if (!enableInteractionTimeout)
        {
            return;
        }

        _interactionTimeoutRoutine = StartCoroutine(InteractionTimeoutRoutine());
    }

    private void StopInteractionTimeout()
    {
        if (_interactionTimeoutRoutine == null)
        {
            return;
        }

        StopCoroutine(_interactionTimeoutRoutine);
        _interactionTimeoutRoutine = null;
    }

    private System.Collections.IEnumerator InteractionTimeoutRoutine()
    {
        float elapsed = 0f;

        while (IsActive && !IsComplete && elapsed < interactionTimeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _interactionTimeoutRoutine = null;

        if (!IsActive || IsComplete)
        {
            yield break;
        }

        Debug.Log($"[{name}] Interaction timed out after {interactionTimeoutSeconds:0.##} seconds. Completing automatically.");
        Complete();
    }

    private static void SetEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null) return;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null) continue;
            behaviours[i].enabled = enabled;
        }
    }

    private static void SetActive(GameObject[] gameObjects, bool active)
    {
        if (gameObjects == null) return;

        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            gameObjects[i].SetActive(active);
        }
    }
}
