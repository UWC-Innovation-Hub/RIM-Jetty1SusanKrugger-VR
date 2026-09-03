using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConversationTrigger : MonoBehaviour, IConversationGazeTarget
{
    [Header("Wiring")]
    [SerializeField] private CharacterInteractionModule interaction;
    [SerializeField] private CharacterConversation character;

    [Header("Settings")]
    [Tooltip("This will trigger the conversation")]
    [SerializeField] private string playerTag = "Player";

    [SerializeField] private float gazeConfirmDelay = 0.5f;

    private bool _isPlayerInTrigger;
    private Coroutine _gazeConfirmRoutine;


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CharacterTrigger] OnTriggerEnter fired on '{name}' by '{other.name}' (tag: {other.tag}. Collider enabled: {GetComponent<Collider>().enabled}");

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _isPlayerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _isPlayerInTrigger = false;
        StopGaze();
    }

    public void OnGazeEnter()
    {
        Debug.Log("Looking at character");

        if (_gazeConfirmRoutine != null)
        {
            return;
        }

        _gazeConfirmRoutine = StartCoroutine(GazeConfirmRoutine());
    }

    public void OnGazeExit()
    {
        StopGaze();
    }

    private IEnumerator GazeConfirmRoutine()
    {
        yield return new WaitForSeconds(gazeConfirmDelay);

        _gazeConfirmRoutine = null;

        if (_isPlayerInTrigger)
        {
            interaction.StartConversation(character);
        }
    }

    private void StopGaze()
    {
        if (_gazeConfirmRoutine != null)
        {
            StopCoroutine(_gazeConfirmRoutine);
            _gazeConfirmRoutine = null;
        }
    }
}
