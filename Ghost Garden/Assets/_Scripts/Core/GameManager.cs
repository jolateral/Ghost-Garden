using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public int maxDays = 10;
    public int currentDay = 1;
    public bool gameWon = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called by DayNightCycle at the end of each day
    public void AdvanceDay()
    {
        currentDay++;
        NudgeSystem.Instance?.ResetNudges();
        HUDManager.Instance?.UpdateDayDisplay(currentDay, maxDays);

        if (currentDay > maxDays)
            TriggerLose();
    }

    public void TriggerWin()
    {
        gameWon = true;
        HUDManager.Instance?.ShowWinScreen();
    }

    public void TriggerLose()
    {
        HUDManager.Instance?.ShowLoseScreen();
    }

    public void ReturnToTitle() => SceneManager.LoadScene("TitleScreen");
}