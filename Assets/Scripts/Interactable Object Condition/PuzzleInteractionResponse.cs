using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PuzzleInteractionResponse
{
    [Header("Condition")]
    public List<FlagCondition> conditions = new List<FlagCondition>();

    [Header("Dialogue")]
    public BoxDialogue dialougeData;

    [Header("Result")]
    public InteractionResultType resultType = InteractionResultType.DialougeOnly;
    public InventoryItemData itemToPickup;

    [Header("After Interacton")]
    public List<FlagAction> actionsAfterComplete = new List<FlagAction>();

    public bool ConditionsMet()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i] != null && !conditions[i].IsMet())
                return false;
        }

        return true;
    }

}
