using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using FMODUnity; // Required for FMOD integration

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("HUD Text")]
    public TextMeshProUGUI nudgeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI messageText;

    [Header("Panels")]
    public CanvasGroup winPanel;
    public CanvasGroup losePanel;

    [Header("Navigation Settings")]
    public string titleScreenName = "TitleScreen"; 
    public Button winHomeButton;
    public Button loseHomeButton;

    [Header("Audio Events")]
    // Assign your pop sound from the Master.bank here in the Inspector
    [SerializeField] private EventReference buttonPopEvent;

    [Header("Fade Settings")]
    public float panelFadeDuration = 1f;
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

        SetupPanel(winPanel);
        SetupPanel(losePanel);

        if (winHomeButton)  winHomeButton.onClick.AddListener(GoToTitleScreen);
        if (loseHomeButton) loseHomeButton.onClick.AddListener(GoToTitleScreen);
    }

    private void SetupPanel(CanvasGroup panel)
    {
        if (panel != null)
        {
            panel.alpha          = 0f;
            panel.interactable   = false;
            panel.blocksRaycasts = false;
            panel.gameObject.SetActive(false);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        if (winPanel) 
        {
            UnlockCursor();
            StartCoroutine(FadeInPanel(winPanel));
        }
    }

    public void ShowLoseScreen()
    {
        if (losePanel) 
        {
            UnlockCursor();
            StartCoroutine(FadeInPanel(losePanel));
        }
    }

    IEnumerator FadeInPanel(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.alpha          = 0f;
        panel.interactable   = false;
        panel.blocksRaycasts = false;

        yield return new WaitForSeconds(panelFadeDelay);

        float elapsed = 0f;
        while (elapsed < panelFadeDuration)
        {
            elapsed      += Time.deltaTime;
            panel.alpha   = Mathf.Clamp01(elapsed / panelFadeDuration);
            yield return null;
        }

        panel.alpha          = 1f;
        panel.interactable   = true;   
        panel.blocksRaycasts = true;
    }

    // ── Navigation & Audio ──────────────────────────────────────────────────

    void GoToTitleScreen()
    {
        // Play the pop sound only when the button is clicked
        PlaySound(buttonPopEvent);
        
        StartCoroutine(FadeToBlackAndLoad());
    }

    IEnumerator FadeToBlackAndLoad()
    {
        if (winHomeButton)  winHomeButton.interactable  = false;
        if (loseHomeButton) loseHomeButton.interactable = false;

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

        SceneManager.LoadScene(titleScreenName);
    }

    private void PlaySound(EventReference eventRef)
    {
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef);
        }
    }
}