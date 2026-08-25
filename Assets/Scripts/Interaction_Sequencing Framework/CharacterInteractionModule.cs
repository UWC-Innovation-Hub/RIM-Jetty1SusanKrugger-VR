using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class CharacterInteractionModule : InteractionModuleBase
{
    [Header("Characters (assign all four)")]
    [SerializeField] private CharacterConversation[] characters = new CharacterConversation[0];

    [Header("Tutorial")]
    [SerializeField] private TutorialPopup tutorialPopup;
    [SerializeField] private float tutorialTimeout = 15f;

    [Header("Interaction Timeout")]
    [SerializeField] private float interactionTimeout = 15f;

    [Header("Events")]
    [Tooltip("Fired each time a conversation is complete")]
    [SerializeField] private UnityEvent<int> onCharacterCompleted;

    [Tooltip("Fired when all characters have been spoken to")]
    [SerializeField] private UnityEvent onAllCharactersCompleted;

    //RUNTIME STATE

    public int CompletedCount { get; private set; }
    public int TotalCount => characters.Length;

    public CharacterConversation ActiveCharacter { get; private set; }

    private readonly HashSet<CharacterConversation> _completedCharacters = new HashSet<CharacterConversation>();
    private Coroutine _tutorialTimeoutRoutine;
    private Coroutine _interactionTimeoutRoutine;
    private bool _tutorialResolved;

    //INTERACTIONMODULEBASE OVERRIDES

    public override void Activate()
    {
        base.Activate();

        CompletedCount = 0;
        ActiveCharacter = null;
        _tutorialResolved = false;
        _completedCharacters.Clear();

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                characters[i].Deactivate();
            }
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed += OnTutorialClosed;
            tutorialPopup.Show();

            if (tutorialTimeout > 0f)
            {
                _tutorialTimeoutRoutine = StartCoroutine(TutorialTimeoutRoutine());
            }
        }
        else
        {
            ShowAllHighlights();
        }
    }

    public override void Deactivate()
    {
        Debug.Log($"[CharacterInteractionModule] Deactivate() called. Stack trace:\n{System.Environment.StackTrace}");

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed -= OnTutorialClosed;
        }

        if (_tutorialTimeoutRoutine != null)
        {
            StopCoroutine(_tutorialTimeoutRoutine);
            _tutorialTimeoutRoutine = null;
        }

        if (_interactionTimeoutRoutine != null)
        {
            StopCoroutine(_interactionTimeoutRoutine);
            _interactionTimeoutRoutine = null;
        }

        if (ActiveCharacter != null)
        {
            ActiveCharacter.Completed -= OnCharacterCompleted;
            ActiveCharacter.Deactivate();
            ActiveCharacter = null;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                characters[i].HideHighlight();
            }
        }


        base.Deactivate();
    }

    //PUBLIC API

    public void StartConversation(CharacterConversation character)
    {
        if (!IsActive || IsComplete)
        {
            Debug.LogWarning($"[CharacterInteractionModule] StartConversation called " + $"but is not active or is completed");
            return;
        }

        if (ActiveCharacter != null)
        {
            Debug.LogWarning($"[MultiCharacterConversationModule] Already in a conversation with " + $"'{ActiveCharacter.CharacterName}'. Finish that one first.");
            return;
        }

        if (character == null || !IsRegisteredCharacter(character))
        {
            Debug.LogWarning($"[MultiCharacterConversationModule] Character '{character?.CharacterName}' " + $"is not registered with this module");
            return;

        }

        if (_completedCharacters.Contains(character))
        {
            Debug.Log($"[MultiCharacterConversationModule] '{character.CharacterName}' has already " + $"been spoken to. Ignoring.");
            return;
        }

        BeginConversation(character);
    }

    //INTERNAL HELPERS

    private void OnTutorialClosed()
    {
        ResolveTutorial();
    }

    private IEnumerator TutorialTimeoutRoutine()
    {
        yield return new WaitForSeconds(tutorialTimeout);

        _tutorialTimeoutRoutine = null;

        if (_tutorialResolved)
        {
            yield break;
        }

        if (tutorialPopup != null)
        {
            tutorialPopup.Hide();
        }

        ResolveTutorial();
    }

    private void ResolveTutorial()
    {
        if (_tutorialResolved)
        {
            return;
        }

        Debug.Log("[CharacterInteractionModule] ResolveTutorial() called.");

        _tutorialResolved = true;

        if (tutorialPopup != null)
        {
            tutorialPopup.Closed -= OnTutorialClosed;
        }

        if (_tutorialTimeoutRoutine != null)
        {
            StopCoroutine(_tutorialTimeoutRoutine);
            _tutorialTimeoutRoutine = null;
        }

        ShowAllHighlights();
    }

    private void ShowAllHighlights()
    {
        Debug.Log($"[CharacterInteractionModule] ShowAllHighlights() called. interactionTimeout: {interactionTimeout}");

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                characters[i].ShowHighlight();
            }
        }

        if (interactionTimeout > 0f)
        {
            if (_interactionTimeoutRoutine != null)
            {
                StopCoroutine(_interactionTimeoutRoutine);
            }

            Debug.Log($"[CharacterInteractionModule] Starting interaction timeout ({interactionTimeout}s).");
            _interactionTimeoutRoutine = StartCoroutine(InteractionTimeoutRoutine());
        }
    }

    private void BeginConversation(CharacterConversation character)
    {
        if (_interactionTimeoutRoutine != null)
        {
            StopCoroutine(_interactionTimeoutRoutine);
            _interactionTimeoutRoutine = null;
        }

        ActiveCharacter = character;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i] != character)
            {
                characters[i]. HideHighlight();
            }
        }

        character.Completed -= OnCharacterCompleted;
        character.Completed += OnCharacterCompleted;

        character.Activate();
    }

    private void OnCharacterCompleted()
    {
        CharacterConversation finished = ActiveCharacter;

        if (finished != null)
        {
            finished.Completed -= OnCharacterCompleted;
            finished.Deactivate();
            finished.HideHighlight();
            _completedCharacters.Add(finished);
            CompletedCount = _completedCharacters.Count;
        }

        ActiveCharacter = null;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && !_completedCharacters.Contains(characters[i]))
            {
                characters[i]. ShowHighlight();
            }
        }

        if (interactionTimeout > 0f && CompletedCount < characters.Length)
        {
            if (_interactionTimeoutRoutine != null)
            {
                StopCoroutine(_interactionTimeoutRoutine);
            }

            _interactionTimeoutRoutine = StartCoroutine(InteractionTimeoutRoutine());
        }

        onCharacterCompleted?.Invoke(CompletedCount);

        if (CompletedCount >= characters.Length)
        {
            Debug.Log("[CharacterInteractionModule] All characters spoken to.");
            onAllCharactersCompleted?.Invoke();
            Complete();
        }
    }

    private bool IsRegisteredCharacter(CharacterConversation character)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == character)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator InteractionTimeoutRoutine()
    {
        yield return new WaitForSeconds(interactionTimeout);

        _interactionTimeoutRoutine = null;

        List<CharacterConversation> remaining = new List<CharacterConversation>();

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && !_completedCharacters.Contains(characters[i]))
            {
                remaining.Add(characters[i]);
            }
        }

        if (remaining.Count == 0)
        {
            yield break;
        }

        CharacterConversation randomCharacter = remaining[Random.Range(0, remaining.Count)];
        Debug.Log($"[CharacterInteractionModule] Interaction timed out. Auto-starting conversation with '{randomCharacter.CharacterName}'.");
        StartConversation(randomCharacter);
    }
}