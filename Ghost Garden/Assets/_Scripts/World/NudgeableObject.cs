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
        None,
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
            case NudgeRole.Tarp:
                AudioManager.Instance?.PlayNudge(1f); // correct nudge quality
                WinSequenceManager.Instance?.RegisterTarpNudge();
                break;

            case NudgeRole.WateringCan:
                AudioManager.Instance?.PlayNudge(1f);
                WinSequenceManager.Instance?.RegisterWateringCanNudge();
                break;

            case NudgeRole.Windchimes:
                // Windchime audio is handled inside WindchimeAnimator.Sway()
                // so we only play the nudge sound if it fails
                bool ok = WinSequenceManager.Instance != null &&
                          NeighbourAI.Instance != null &&
                          NeighbourAI.Instance.IsWalking &&
                          NeighbourAI.Instance.IsNearHouse;
                AudioManager.Instance?.PlayNudge(ok ? 1f : 0f);
                WinSequenceManager.Instance?.RegisterWindchimeNudge();
                break;

            case NudgeRole.None:
                AudioManager.Instance?.PlayNudge(0f); // ambient/wrong nudge
                HUDManager.Instance?.ShowMessage("...");
                break;
        }
    }
}