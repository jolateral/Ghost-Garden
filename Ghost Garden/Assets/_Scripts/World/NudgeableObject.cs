using UnityEngine;
using UnityEngine.Events;

public class NudgeableObject : MonoBehaviour
{
    public string nudgeName;
    public UnityEvent onNudged; // optional extra events wired in Inspector

    [Header("Win Sequence Role")]
    public NudgeRole role = NudgeRole.None;

    public enum NudgeRole
    {
        None,        // decorative nudge, no win consequence
        Tarp,        // nudging this triggers the birds/tarp sequence
        WateringCan, // nudging this knocks the can off the shelf
        Windchimes   // nudging this rings the chimes (only works when neighbour is near)
    }

    public virtual void Nudge()
    {
        onNudged?.Invoke();
        Debug.Log($"[NudgeableObject] Nudged: {nudgeName}");

        switch (role)
        {
            case NudgeRole.Tarp:
                WinSequenceManager.Instance?.RegisterTarpNudge();
                break;
            case NudgeRole.WateringCan:
                WinSequenceManager.Instance?.RegisterWateringCanNudge();
                break;
            case NudgeRole.Windchimes:
                WinSequenceManager.Instance?.RegisterWindchimeNudge();
                break;
            case NudgeRole.None:
                // Play a small ambient reaction so the world feels alive
                HUDManager.Instance?.ShowMessage("...");
                break;
        }
    }
}