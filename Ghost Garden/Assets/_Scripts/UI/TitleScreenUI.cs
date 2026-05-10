using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenUI : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeGroup; // Assign a full-screen Black Image's CanvasGroup here
    public float fadeDuration = 1.5f;

    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStart);
        quitButton.onClick.AddListener(OnQuit);

        // Ensure the black screen is blocking everything at the start
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true; 

        StartCoroutine(FadeFromBlack());
    }

    IEnumerator FadeFromBlack()
    {
        float t = fadeDuration;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false; // Allow clicking buttons after fade
    }

    void OnStart()
    {
        StartCoroutine(FadeToBlackAndLoad());
    }

    IEnumerator FadeToBlackAndLoad()
    {
        // Disable buttons immediately so they can't be clicked twice
        startButton.interactable = false;
        quitButton.interactable = false;
        
        fadeGroup.blocksRaycasts = true; // Block input during fade

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }

        fadeGroup.alpha = 1f;
        SceneManager.LoadScene("Garden");
    }

    void OnQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}