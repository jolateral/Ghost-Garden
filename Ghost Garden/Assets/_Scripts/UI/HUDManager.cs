using UnityEngine;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("HUD Text")]
    public TextMeshProUGUI nudgeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI messageText;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialise display with starting values
        UpdateNudgeDisplay(NudgeSystem.Instance.NudgesRemaining,
                           NudgeSystem.Instance.nudgesPerDay);
        UpdateDayDisplay(GameManager.Instance.currentDay,
                         GameManager.Instance.maxDays);
        messageText.gameObject.SetActive(false);
        if (winPanel)  winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    public void UpdateNudgeDisplay(int remaining, int total)
    {
        if (nudgeText != null)
            nudgeText.text = $"Nudges: {remaining} / {total}";
    }

    public void UpdateDayDisplay(int current, int max)
    {
        if (dayText != null)
            dayText.text = $"Day {current} / {max}";
    }

    public void ShowMessage(string msg, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTemp(msg, duration));
    }

    IEnumerator ShowTemp(string msg, float dur)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(dur);
        messageText.gameObject.SetActive(false);
    }

    public void ShowWinScreen()
    {
        if (winPanel) winPanel.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        if (losePanel) losePanel.SetActive(true);
    }
}