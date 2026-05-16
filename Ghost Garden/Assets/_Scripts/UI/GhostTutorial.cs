using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GhostTutorial : MonoBehaviour
{
    [System.Serializable]
    public class TutorialMessage
    {
        public TextMeshPro textObject;
        [Tooltip("How long the text stays fully visible.")]
        public float holdDuration = 3.5f;
        [Tooltip("How long the fade in / fade out each take.")]
        public float fadeDuration = 1.2f;
    }

    [Header("Messages — arrange in the order they should appear")]
    public List<TutorialMessage> messages = new List<TutorialMessage>();

    [Header("Timing")]
    [Tooltip("Pause before the very first message appears.")]
    public float openingDelay = 2f;
    [Tooltip("Gap between one message fading out and the next fading in.")]
    public float gapBetweenMessages = 0.6f;

    [Header("Player")]
    [Tooltip("Drag your player GameObject here — the one with PlayerController on it.")]
    public PlayerController playerController;

    void Start()
    {
        foreach (var m in messages)
        {
            if (m.textObject == null) continue;
            SetAlpha(m.textObject, 0f);
        }

        StartCoroutine(PlayTutorial());
    }

    IEnumerator PlayTutorial()
    {
        // Lock input for the whole tutorial
        playerController?.SetInputEnabled(false);

        yield return new WaitForSeconds(openingDelay);

        for (int i = 0; i < messages.Count; i++)
        {
            var m = messages[i];
            if (m.textObject == null) continue;

            yield return StartCoroutine(Fade(m.textObject, 0f, 1f, m.fadeDuration));
            yield return new WaitForSeconds(m.holdDuration);
            yield return StartCoroutine(Fade(m.textObject, 1f, 0f, m.fadeDuration));

            yield return new WaitForSeconds(gapBetweenMessages);
        }

        // Unlock input once all messages are done
        playerController?.SetInputEnabled(true);
    }

    IEnumerator Fade(TextMeshPro tmp, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(tmp, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(tmp, to);
    }

    void SetAlpha(TextMeshPro tmp, float a)
    {
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }
}