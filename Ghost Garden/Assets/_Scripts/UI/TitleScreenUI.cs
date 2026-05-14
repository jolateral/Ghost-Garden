using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using FMODUnity; // Required for FMOD integration

public class TitleScreenUI : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeGroup; 
    public float fadeDuration = 1.5f;

    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Audio Events")]
    [SerializeField] private EventReference menuWhooshEvent;
    [SerializeField] private EventReference buttonPopEvent;

    void Start()
    {
        startButton.onClick.AddListener(OnStart);
        quitButton.onClick.AddListener(OnQuit);

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
        fadeGroup.blocksRaycasts = false; 
    }

    void OnStart()
    {
        // Play Pop sound for the button click
        PlaySound(buttonPopEvent);
        
        // Play Whoosh sound as it begins fading to black
        PlaySound(menuWhooshEvent);

        StartCoroutine(FadeToBlackAndLoad());
    }

    IEnumerator FadeToBlackAndLoad()
    {
        startButton.interactable = false;
        quitButton.interactable = false;
        
        fadeGroup.blocksRaycasts = true; 

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
        // Play Pop sound for the quit button click
        PlaySound(buttonPopEvent);

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Helper method to play FMOD events one-shot
    private void PlaySound(EventReference eventRef)
    {
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef);
        }
    }
}