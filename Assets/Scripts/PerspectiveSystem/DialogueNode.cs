using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Speaker")]
    public string speakerName;

    [Header("Dialogue")]
    [TextArea(3, 6)]
    public string dialogueText;

    [Header("Portrait")]
    public Sprite portrait;

    [Header("Normal Next Dialogue")]
    public int nextNodeIndex = -1;

    [Header("Player Choices")]
    public bool hasChoices = false;

    public DialogueChoice[] choices;
}