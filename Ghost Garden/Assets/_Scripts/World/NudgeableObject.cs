// Assets/_Scripts/World/NudgeableObject.cs
using UnityEngine;
using UnityEngine.Events;

public class NudgeableObject : MonoBehaviour
{
    public string nudgeName;
    public UnityEvent onNudged; // wire up in Inspector
    public bool isPartOfWinSequence;
    public int sequenceStep; // 0 = birds, 1 = tarp, 2 = windchimes, 3 = watering can

    public virtual void Nudge()
    {
        onNudged?.Invoke();
        if (isPartOfWinSequence)
            WinSequenceManager.Instance?.RegisterNudge(sequenceStep);
        Debug.Log($"Nudged: {nudgeName}");
    }
}