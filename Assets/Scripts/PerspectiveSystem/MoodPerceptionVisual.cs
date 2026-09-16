using UnityEngine;

[System.Serializable]
public class MoodPerceptionVisual
{
    [Header("Mood")]
    public PatchMood mood = PatchMood.None;

    [Header("World Visual")]
    public Sprite sprite;

    [Header("Dialogue Portrait")]
    public Sprite dialoguePortrait;
}