using UnityEngine;
using System;

[Serializable]
public class FlagCondition
{
    public string flagName;
    public bool expectedValue = true;

    public bool IsMet()
    {
        if (string.IsNullOrEmpty(flagName))
            return true;

        if (PuzzleFlagManager.Instance == null)
            return false;

        return PuzzleFlagManager.Instance.GetFlag(flagName) == expectedValue;
    }
}
