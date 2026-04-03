using UnityEngine;
using System.Collections.Generic;

public class PuzzleFlagManager : MonoBehaviour
{
    public static PuzzleFlagManager Instance { get; private set; }

    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool GetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
            return false;

        return flags.TryGetValue(flagName, out bool value) && value;
    }

    public void SetFlag(string flagName, bool value)
    {
        if (string.IsNullOrEmpty(flagName))
            return;

        flags[flagName] = value;
        Debug.Log($"Flag Changed: {flagName} = {value}");
    }

}
