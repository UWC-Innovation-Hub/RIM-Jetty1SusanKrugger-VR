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

    [SerializeField] private float gazeDeacyRate = 2f;

    private bool _isPlayerInTrigger;
    private bool _isGazing;
    private float _gazeTimer;
    private bool _conversationStarted;


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CharacterTrigger] OnTriggerEnter fired on '{name}' by '{other.name}' (tag: {other.tag}. Collider enabled: {GetComponent<Collider>().enabled}");

        if (other.CompareTag(playerTag))
        {
            _isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _isPlayerInTrigger = false;
        _gazeTimer = 0f;
        _conversationStarted = false;
    }

    public void OnGazeEnter() => _isGazing = true;

    public void OnGazeExit() => _isGazing = false;

    private void Update()
    {
        if (_conversationStarted)
        {
            return;
        }

        if (_isPlayerInTrigger && _isGazing)
        {
            Debug.Log("Convo not starting");
            _gazeTimer += Time.deltaTime;
        }
        else
        {
            _gazeTimer -= gazeDeacyRate * Time.deltaTime;
        }

        _gazeTimer = Mathf.Clamp(_gazeTimer, 0f, gazeConfirmDelay);

        if (_gazeTimer >= gazeConfirmDelay)
        {
            _conversationStarted = true;
            
            interaction.StartConversation(character);

        }
    }
}
