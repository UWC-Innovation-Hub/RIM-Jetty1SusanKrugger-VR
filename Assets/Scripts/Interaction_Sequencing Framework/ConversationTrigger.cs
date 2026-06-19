using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConversationTrigger : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CharacterInteractionModule interaction;
    [SerializeField] private CharacterConversation character;

    [Header("Settings")]
    [Tooltip("This will trigger the conversation")]
    [SerializeField] private string playerTag = "Player";


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        interaction.StartConversation(character);
    }
}
