using UnityEngine;
using System;

[Serializable]

public class FlagAction
{
    public string flagName;
    public bool setValue = true;

    public void Execute()
    {
        if (PuzzleFlagManager.Instance == null)
            return;

        PuzzleFlagManager.Instance.SetFlag(flagName, setValue);
    }
}
