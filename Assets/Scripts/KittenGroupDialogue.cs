using UnityEngine;

[CreateAssetMenu(fileName = "New Kitten Group Dialogue", menuName = "Dialogue/Kitten Group Dialogue")]
public class KittenGroupDialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;

        [TextArea(2, 5)]
        public string dialogueText;
    }

    public DialogueLine[] dialogueLines;

    public float typingSpeed = 0.07f;
}
