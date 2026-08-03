using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterConversation : InteractionModuleBase
{
    [Header("Character Info")]
    [SerializeField] private string characterName = "Character";

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Spotlight")]
    [SerializeField] private CharacterSpotlight spotlight;

    [Header("Highlight")]
    [SerializeField] private CharacterHighlight highlight;

    [Header("Conversation Complete Event")]
    [SerializeField] private UnityEvent onConversationEnded;

    [Header("Animation")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private string talkingTrigger = "StartTalking";
    [SerializeField] private string idleTrigger = "StopTalking";

    [Header("Facial Animation")]
    [SerializeField] private Animator facialAnimator;
    [SerializeField] private string faceTalkingTrigger = "StartTalking";

    public string CharacterName => characterName;

    private Coroutine _dialogueRoutine;

    //INTERACTIONMODULEBASE OVERRIDES

    public override void Activate()
    {
        Debug.Log($"[CharacterConversation] Activate() called on '{characterName}'. highlight assigned: {highlight != null}");

        base.Activate();

        if (spotlight != null)
        {
            spotlight.Activate();
        }

        if (highlight != null)
        {
            Debug.Log($"[CharacterConversation] Calling highlight.Hide() on '{characterName}'.");
            highlight.Hide();
        }
        else
        {
            Debug.LogWarning($"[CharacterConversation] '{characterName}' has no highlight assigned � cannot fade it out.");
        }

        if (_dialogueRoutine != null)
        {
            StopCoroutine(_dialogueRoutine);
        }

        _dialogueRoutine = StartCoroutine(DialogueRoutine());
    }

    public override void Deactivate()
    {
        if (_dialogueRoutine != null)
        {
            StopCoroutine(_dialogueRoutine);
            _dialogueRoutine = null;
        }

        if (spotlight != null)
        {
            spotlight.Deactivate();
        }

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        SetIdle();

        base.Deactivate();
    }

    //PUBLIC API

    public void FinishConversation()
    {
        if (!IsActive || IsComplete)
        {
            return;
        }
        onConversationEnded?.Invoke();
        Complete();
    }

    public void ShowHighlight()
    {
        if (highlight != null)
        {
            highlight.Show();
        }
    }

    public void HideHighlight()
    {
        if (highlight != null)
        {
            highlight.Hide();
        }
    }

    //ANIMATION HELPERS

    private void SetTalking()
    {
        if (bodyAnimator != null)
        {
            bodyAnimator.SetTrigger(talkingTrigger);
        }

        if (facialAnimator != null)
        {
            facialAnimator.SetTrigger(faceTalkingTrigger);
        }
    }

    private void SetIdle()
    {
        if (bodyAnimator != null)
        {
            bodyAnimator.SetTrigger(idleTrigger);
        }
    }

    //DIALOGUE FLOW

    private IEnumerator PlayClipAndWait(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            yield break;
        }

        Debug.Log($"[CharacterConversation] '{characterName}' starting clip '{clip.name}' (length: {clip.length:0.00}s).");

        SetTalking();

        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length);

        SetIdle();
    }

    private IEnumerator DialogueRoutine()
    {
        if (dialogueData == null)
        {
            FinishConversation();
            yield break;
        }

        if (dialogueData.openingClip != null && audioSource != null)
        {
            yield return PlayClipAndWait(dialogueData.openingClip);
        }
        else
        {
            if (dialogueData.openingClip == null)
            {
                Debug.LogWarning($"[CharacterConversation] '{characterName}' has no opening clip assigned.");
            }

            if (audioSource == null)
            {
                Debug.LogWarning($"[CharacterConversation] '{characterName}' has no AudioSource assigned.");
            }
        }

        if (dialogueData.choices == null || dialogueData.choices.Length == 0)
        {
            Debug.LogWarning($"[CharacterConversation] '{characterName}' DialogueData has no choices.");
            FinishConversation();
            yield break;
        }

        int selectedIndex = -1;

        void OnChoice(int index) => selectedIndex = index;
        dialogueUI.OnChoiceSelected += OnChoice;
        dialogueUI.Show(dialogueData.choices);

        yield return new WaitUntil(() => selectedIndex >= 0);

        dialogueUI.OnChoiceSelected -= OnChoice;
        dialogueUI.Hide();

        DialogueChoice chosen = dialogueData.choices[selectedIndex];

        if (chosen.replyClip != null && audioSource != null)
        {
            yield return PlayClipAndWait(chosen.replyClip);
        }

        _dialogueRoutine = null;
        FinishConversation();
    }
}