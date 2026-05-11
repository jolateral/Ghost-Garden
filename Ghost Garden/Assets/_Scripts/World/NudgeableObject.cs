using UnityEngine;
using UnityEngine.Events;

public class NudgeableObject : MonoBehaviour
{
    public string nudgeName;
    public UnityEvent onNudged;

    [Header("Win Sequence Role")]
    public NudgeRole role = NudgeRole.None;

    public enum NudgeRole
    {
        None,        // decorative — plays nudge sound + squish animation
        Foliage,     // trees/flowers — plays foliage brush sound + squish animation
        Tarp,
        WateringCan,
        Windchimes
    }

    public virtual void Nudge()
    {
        onNudged?.Invoke();
        Debug.Log($"[NudgeableObject] Nudged: {nudgeName}");

        switch (role)
        {
            case NudgeRole.None:
                AudioManager.Instance?.PlayNudge(0f);
                HUDManager.Instance?.ShowMessage("...");
                break;

            case NudgeRole.Foliage:
                // Foliage brush sound — no HUD message, just rustling
                AudioManager.Instance?.PlayFoliageBrush(transform.position);
                HUDManager.Instance?.ShowMessage("...");
                break;

            case NudgeRole.Tarp:
                AudioManager.Instance?.PlayNudge(1f);
                WinSequenceManager.Instance?.RegisterTarpNudge();
                break;

            case NudgeRole.WateringCan:
                AudioManager.Instance?.PlayNudge(1f);
                WinSequenceManager.Instance?.RegisterWateringCanNudge();
                break;

            case NudgeRole.Windchimes:
                bool ok = WinSequenceManager.Instance != null &&
                          NeighbourAI.Instance != null &&
                          NeighbourAI.Instance.IsWalking &&
                          NeighbourAI.Instance.IsNearHouse;
                AudioManager.Instance?.PlayNudge(ok ? 1f : 0f);
                WinSequenceManager.Instance?.RegisterWindchimeNudge();
                break;
        }
    }
}