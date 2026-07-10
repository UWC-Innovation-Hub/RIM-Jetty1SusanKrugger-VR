using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public AudioClip openingClip;

    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string responseText;

    public AudioClip replyClip;
}
