using UnityEngine;

public class NPCDialogueConversation : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueNode[] dialogueNodes;

    [Header("Starting Node")]
    public int startingNodeIndex = 0;

    [Header("Starting Perception")]
    public NPCPerspectiveSetup perspectiveSetup;
}