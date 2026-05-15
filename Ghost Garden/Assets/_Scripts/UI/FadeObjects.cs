using UnityEngine;
using System.Collections;

public class FadeObject : MonoBehaviour
{
    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        // Start by fading in, then fading out
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        yield return StartCoroutine(Fade(0, 1, 2.0f)); // Fade In over 2 seconds
        yield return new WaitForSeconds(5);            // Wait 1 second
        yield return StartCoroutine(Fade(1, 0, 2.0f)); // Fade Out over 2 seconds
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        Color color = mat.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            mat.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }
    }
}
