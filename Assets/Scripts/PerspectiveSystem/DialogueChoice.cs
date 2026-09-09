using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string choiceText;

    public int nextNodeIndex = -1;
}