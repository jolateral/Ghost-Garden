// Assets/_Scripts/UI/TitleScreenUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenUI : MonoBehaviour
{
    public CanvasGroup fadeGroup; // on root Canvas
    public Button startButton;
    public Button quitButton;
    public float fadeDuration = 1.5f;

    void Start()
    {
        startButton.onClick.AddListener(OnStart);
        quitButton.onClick.AddListener(OnQuit);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }
        fadeGroup.alpha = 1f;
    }

    void OnStart() => StartCoroutine(FadeOutAndLoad());

    IEnumerator FadeOutAndLoad()
    {
        startButton.interactable = false;
        quitButton.interactable = false;
        float t = fadeDuration;
        while (t > 0f) {
            t -= Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }
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