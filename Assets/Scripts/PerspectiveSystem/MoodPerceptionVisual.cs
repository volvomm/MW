using UnityEngine;

[System.Serializable]
public class MoodPerceptionVisual
{
    [Header("Mood")]
    public PatchMood mood = PatchMood.None;

    [Header("Visual")]
    public Sprite sprite;
}
