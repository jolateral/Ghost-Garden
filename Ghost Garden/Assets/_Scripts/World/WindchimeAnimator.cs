using UnityEngine;
using System.Collections;

// Attach this to your windchime GameObject.
// It plays a gentle swaying animation when the chimes are hit.

public class WindchimeAnimator : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAngle = 15f;
    public float swaySpeed = 2f;
    public float swayDuration = 3f;

    bool _swaying;

    // Called by WinSequenceManager
    public void Sway()
    {
        if (_swaying) return;
        StartCoroutine(SwayRoutine());
    }

    IEnumerator SwayRoutine()
    {
        _swaying = true;
        float elapsed = 0f;
        Quaternion startRot = transform.localRotation;

        while (elapsed < swayDuration)
        {
            float angle = Mathf.Sin(elapsed * swaySpeed * Mathf.PI) 
                        * swayAngle 
                        * (1f - elapsed / swayDuration); // fade out sway over time

            transform.localRotation = startRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        _swaying = false;
    }
}