using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [Header("Choice Text")]
    [TextArea(1, 3)]
    public string choiceText;

    [Header("Next Dialogue")]
    public int nextNodeIndex = -1;

    [Header("Mood Consequence")]
    public bool changeMood = false;
    public PatchMood newMood = PatchMood.None;

    [Header("Perception Consequence")]
    public bool triggerPerceptionChange = false;

    [Tooltip("How long the dialogue disappears before the visual change happens.")]
    public float perceptionDelay = 2f;

    [Header("Perception Change Type")]
    public PerceptionChangeType perceptionChangeType =
        PerceptionChangeType.SpriteSwap;

    [Header("Sprite Swap Settings")]
    public SpriteRenderer perceptionTarget;
    public Sprite replacementSprite;

    [Header("Animation Settings")]
    public Animator perceptionAnimator;
    public string animationTriggerName = "Transform";

    [Tooltip("How long the transformation animation takes.")]
    public float animationDuration = 1f;
}