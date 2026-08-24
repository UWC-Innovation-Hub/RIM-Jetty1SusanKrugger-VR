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
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                characters[i].ShowHighlight();
            }
        }
    }

    private void BeginConversation(CharacterConversation character)
    {
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
}