using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialSequence : MonoBehaviour
{
    [System.Serializable]
    public class TutorialElement
    {
        public GameObject tutorialObject; // Drag your Canvas or TextMeshPro object here
        public float holdDuration = 2f;   // How long it stays fully visible
        public float fadeDuration = 1f;   // How long the fade in/out takes
    }

    [Header("Tutorial Elements")]
    public List<TutorialElement> tutorialElements = new List<TutorialElement>();

    [Header("Settings")]
    public float delayBetweenElements = 0.3f; // Gap between one fading out and next fading in
    public float startDelay = 1f;             // Delay before the first element appears

    void Start()
    {
        // Hide everything at the start
        foreach (var element in tutorialElements)
        {
            SetAlpha(element.tutorialObject, 0f);
            element.tutorialObject.SetActive(true);
        }

        StartCoroutine(PlayTutorialSequence());
    }

    IEnumerator PlayTutorialSequence()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (var element in tutorialElements)
        {
            yield return StartCoroutine(FadeElement(element));
            yield return new WaitForSeconds(delayBetweenElements);
        }
    }

    IEnumerator FadeElement(TutorialElement element)
    {
        // Fade In
        yield return StartCoroutine(Fade(element.tutorialObject, 0f, 1f, element.fadeDuration));

        // Hold
        yield return new WaitForSeconds(element.holdDuration);

        // Fade Out
        yield return StartCoroutine(Fade(element.tutorialObject, 1f, 0f, element.fadeDuration));
    }

    IEnumerator Fade(GameObject obj, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(obj, alpha);
            yield return null;
        }

        SetAlpha(obj, endAlpha);
    }

    // Handles both World Space TextMeshPro and Screen Space Canvas
    void SetAlpha(GameObject obj, float alpha)
    {
        // Try TextMeshPro (world space floating text)
        TextMeshPro tmp = obj.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
            return;
        }

        // Try TextMeshProUGUI (canvas-based)
        TextMeshProUGUI tmpUGUI = obj.GetComponent<TextMeshProUGUI>();
        if (tmpUGUI != null)
        {
            Color c = tmpUGUI.color;
            c.a = alpha;
            tmpUGUI.color = c;
            return;
        }

        // Try Canvas Group (works for entire Canvas with multiple children)
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
            return;
        }

        // Try to find a CanvasGroup on children
        CanvasGroup childCg = obj.GetComponentInChildren<CanvasGroup>();
        if (childCg != null)
        {
            childCg.alpha = alpha;
        }
    }
}