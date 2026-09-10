using System.Collections;
using UnityEngine;

public class DialogueNPCExitMover : MonoBehaviour
{
    [Header("NPC")]
    public Transform npcTransform;

    [Header("Exit Target")]
    public Transform exitPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Animation - Optional")]
    public Animator npcAnimator;

    [Tooltip("Name of the Animator float used for walking. Leave blank if you do not need it.")]
    public string speedParameter = "Speed";

    [Header("End Behaviour")]
    [Tooltip("Turn the NPC off after it reaches the exit point.")]
    public bool disableNPCAtEnd = true;

    private bool isMoving = false;

    public bool IsMoving => isMoving;

    public IEnumerator RunToExit()
    {
        if (npcTransform == null)
        {
            Debug.LogWarning("DialogueNPCExitMover: NPC Transform has not been assigned.");
            yield break;
        }

        if (exitPoint == null)
        {
            Debug.LogWarning("DialogueNPCExitMover: Exit Point has not been assigned.");
            yield break;
        }

        isMoving = true;

        if (npcAnimator != null && !string.IsNullOrEmpty(speedParameter))
        {
            npcAnimator.SetFloat(speedParameter, 1f);
        }

        while (Vector2.Distance(
                   npcTransform.position,
                   exitPoint.position) > 0.05f)
        {
            npcTransform.position = Vector3.MoveTowards(
                npcTransform.position,
                exitPoint.position,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        npcTransform.position = exitPoint.position;

        if (npcAnimator != null && !string.IsNullOrEmpty(speedParameter))
        {
            npcAnimator.SetFloat(speedParameter, 0f);
        }

        isMoving = false;

        if (disableNPCAtEnd)
        {
            npcTransform.gameObject.SetActive(false);
        }
    }
}