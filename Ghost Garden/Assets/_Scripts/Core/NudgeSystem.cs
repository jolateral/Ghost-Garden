using UnityEngine;

// Attach this to the GameManager GameObject alongside GameManager.cs
// It handles the per-day nudge budget and communicates with the HUD.

public class NudgeSystem : MonoBehaviour
{
    public static NudgeSystem Instance;

    [Header("Settings")]
    public int nudgesPerDay = 5;

    int _nudgesRemaining;

    void Awake()
    {
        Instance = this;
        _nudgesRemaining = nudgesPerDay;
    }

    // Called at the start of each new day by DayNightCycle
    public void ResetNudges()
    {
        _nudgesRemaining = nudgesPerDay;
        HUDManager.Instance?.UpdateNudgeDisplay(_nudgesRemaining, nudgesPerDay);
    }

    // Returns true and spends a nudge if one is available
    public bool TrySpendNudge()
    {
        if (_nudgesRemaining <= 0)
        {
            HUDManager.Instance?.ShowMessage("No nudges left today...");
            return false;
        }

        _nudgesRemaining--;
        HUDManager.Instance?.UpdateNudgeDisplay(_nudgesRemaining, nudgesPerDay);
        return true;
    }

    public int NudgesRemaining => _nudgesRemaining;
}