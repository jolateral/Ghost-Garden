using UnityEngine;
using System.Collections;

public class WindchimeAnimator : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAngle    = 18f;
    public float swaySpeed    = 2.5f;
    public float swayDuration = 4f;

    bool _swaying;

    public void Sway()
    {
        if (_swaying) return;

        // Play FMOD windchime sound
        AudioManager.Instance?.PlayWindchime(transform.position);

        StartCoroutine(SwayRoutine());
    }

    IEnumerator SwayRoutine()
    {
        _swaying = true;
        float elapsed   = 0f;
        Quaternion startRot = transform.localRotation;

        while (elapsed < swayDuration)
        {
            // Amplitude fades out over time
            float amplitude = swayAngle * (1f - elapsed / swayDuration);
            float angle     = Mathf.Sin(elapsed * swaySpeed * Mathf.PI) * amplitude;
            transform.localRotation = startRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        _swaying = false;
    }
}