using UnityEngine;

public class ChaseSequenceManager : MonoBehaviour
{
    public static ChaseSequenceManager Instance;

    [Header("Chase Progress")]
    public bool reunionFinished = false;
    public bool chaseIntroFinished = false;
    public bool reachedOutside = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void MarkReunionFinished()
    {
        reunionFinished = true;

        Debug.Log("REUNION FINISHED - Chase can now begin.");
    }

    public void MarkChaseIntroFinished()
    {
        chaseIntroFinished = true;

        Debug.Log("CHASE INTRO FINISHED.");
    }

    public void MarkReachedOutside()
    {
        reachedOutside = true;

        Debug.Log("PATCH REACHED OUTSIDE.");
    }
}