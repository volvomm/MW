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

    [Header("Mood Condition")]
    public bool requireMood = false;
    public PatchMood requiredMood = PatchMood.None;

    [Tooltip("If Patch does not have the required mood, go to this node instead.")]
    public int failedMoodNodeIndex = -1;

    [Header("Normal Next Dialogue")]
    public int nextNodeIndex = -1;

    [Header("Player Choices")]
    public bool hasChoices = false;
    public DialogueChoice[] choices;

    [Header("Mood Event - When This Node Starts")]
    public bool changeMood = false;
    public PatchMood newMood = PatchMood.None;

    [Header("Perception Event - After This Line")]
    public bool triggerPerceptionEvent = false;

    public bool changeMoodDuringPerception = false;
    public PatchMood perceptionMood = PatchMood.None;

    [Tooltip("How long the dialogue disappears before the visual change occurs.")]
    public float perceptionDelay = 2f;

    [Header("Perception Change Type")]
    public PerceptionChangeType perceptionChangeType =
        PerceptionChangeType.SpriteSwap;

    [Header("Sprite Swap Settings")]
    [Tooltip("The SpriteRenderer of the NPC or object that changes appearance.")]
    public SpriteRenderer perceptionTarget;

    [Tooltip("The sprite that the target changes into.")]
    public Sprite replacementSprite;

    [Header("Animation Settings")]
    [Tooltip("Animator used for an animated perception transformation.")]
    public Animator perceptionAnimator;

    [Tooltip("Animator Trigger parameter that starts the transformation.")]
    public string animationTriggerName = "Transform";

    [Tooltip("How long to wait for the transformation animation before dialogue resumes.")]
    public float animationDuration = 1f;

    [Header("NPC Exit Event - After This Line")]
    public bool triggerNPCExit = false;

    [Tooltip("NPC movement sequence that plays when this dialogue line ends.")]
    public DialogueNPCExitMover npcExitMover;
}