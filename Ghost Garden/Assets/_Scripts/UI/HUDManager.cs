using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    // Each panel needs a CanvasGroup component for fading.
    // Add one via Add Component → Canvas Group on each panel.
    public CanvasGroup winPanel;
    public CanvasGroup losePanel;

    [Header("Home Buttons")]
    // Drag the Home button from the Win panel here
    public Button winHomeButton;
    // Drag the Home button from the Lose panel here
    public Button loseHomeButton;

    [Header("Fade Settings")]
    public float panelFadeDuration = 1f;
    // Delay before the panel starts fading in, so the player
    // has a moment to process what just happened
    public float panelFadeDelay   = 0.8f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateNudgeDisplay(NudgeSystem.Instance.NudgesRemaining,
                           NudgeSystem.Instance.nudgesPerDay);
        UpdateDayDisplay(GameManager.Instance.currentDay,
                         GameManager.Instance.maxDays);

        messageText.gameObject.SetActive(false);

        // Hide both panels fully at start
        if (winPanel)
        {
            winPanel.alpha          = 0f;
            winPanel.interactable   = false;
            winPanel.blocksRaycasts = false;
            winPanel.gameObject.SetActive(false);
        }
        if (losePanel)
        {
            losePanel.alpha          = 0f;
            losePanel.interactable   = false;
            losePanel.blocksRaycasts = false;
            losePanel.gameObject.SetActive(false);
        }

        // Wire up home buttons
        if (winHomeButton)  winHomeButton.onClick.AddListener(GoToTitleScreen);
        if (loseHomeButton) loseHomeButton.onClick.AddListener(GoToTitleScreen);
    }

    // ── HUD updates ───────────────────────────────────────────────────────────

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

    // ── Win / Lose panels ─────────────────────────────────────────────────────

    public void ShowWinScreen()
    {
        if (winPanel) StartCoroutine(FadeInPanel(winPanel));
    }

    public void ShowLoseScreen()
    {
        if (losePanel) StartCoroutine(FadeInPanel(losePanel));
    }

    IEnumerator FadeInPanel(CanvasGroup panel)
    {
        // Activate but keep invisible so layout is ready
        panel.gameObject.SetActive(true);
        panel.alpha          = 0f;
        panel.interactable   = false;
        panel.blocksRaycasts = false;

        // Brief pause before fading
        yield return new WaitForSeconds(panelFadeDelay);

        // Fade in
        float elapsed = 0f;
        while (elapsed < panelFadeDuration)
        {
            elapsed      += Time.deltaTime;
            panel.alpha   = Mathf.Clamp01(elapsed / panelFadeDuration);
            yield return null;
        }

        panel.alpha          = 1f;
        panel.interactable   = true;   // buttons are now clickable
        panel.blocksRaycasts = true;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    void GoToTitleScreen()
    {
        StartCoroutine(FadeToBlackAndLoad());
    }

    IEnumerator FadeToBlackAndLoad()
    {
        // Disable both home buttons so they can't be clicked twice
        if (winHomeButton)  winHomeButton.interactable  = false;
        if (loseHomeButton) loseHomeButton.interactable = false;

        // Fade the active panel back out before switching scenes
        CanvasGroup active = (winPanel  != null && winPanel.alpha  > 0f) ? winPanel
                           : (losePanel != null && losePanel.alpha > 0f) ? losePanel
                           : null;

        if (active != null)
        {
            float elapsed = 0f;
            while (elapsed < panelFadeDuration)
            {
                elapsed      += Time.deltaTime;
                active.alpha  = Mathf.Lerp(1f, 0f, elapsed / panelFadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene("TitleScreen");
    }
}