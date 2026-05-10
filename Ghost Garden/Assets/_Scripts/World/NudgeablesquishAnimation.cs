using UnityEngine;
using System.Collections;

// Attach this to any nudgeable object that is NOT part of the win sequence.
// When Squish() is called it flattens the object on the Y axis and
// springs it back to normal size with a little overshoot bounce.
//
// SETUP:
//   1. Attach this script to the nudgeable object
//   2. In NudgeableObject on the same object, set Role = None
//   3. Wire up NudgeableObject's OnNudged UnityEvent to call
//      this script's Squish() method in the Inspector

public class NudgeSquishAnimation : MonoBehaviour
{
    [Header("Squish Shape")]
    // How much the object flattens on Y at peak squish (0.5 = half height)
    public float squishScaleY    = 0.5f;
    // How much it widens on X/Z to conserve volume during squish
    public float squishScaleXZ   = 1.3f;

    [Header("Timing")]
    public float squishDuration  = 0.08f; // how fast it squishes down
    public float springDuration  = 0.18f; // how fast it springs back up
    public float overshootAmount = 0.12f; // how much it overshoots normal size
    public float settleDuration  = 0.1f;  // how fast the overshoot settles

    Vector3 _originalScale;
    bool _animating;

    void Start()
    {
        _originalScale = transform.localScale;
    }

    // Call this from NudgeableObject's OnNudged UnityEvent in the Inspector
    public void Squish()
    {
        if (_animating) return;
        StartCoroutine(SquishRoutine());
    }

    IEnumerator SquishRoutine()
    {
        _animating = true;

        Vector3 squished = new Vector3(
            _originalScale.x * squishScaleXZ,
            _originalScale.y * squishScaleY,
            _originalScale.z * squishScaleXZ);

        Vector3 overshot = new Vector3(
            _originalScale.x * (1f - overshootAmount * 0.5f),
            _originalScale.y * (1f + overshootAmount),
            _originalScale.z * (1f - overshootAmount * 0.5f));

        // Phase 1: squish down
        yield return ScaleTo(squished, squishDuration, EaseIn);

        // Phase 2: spring back up past normal (overshoot)
        yield return ScaleTo(overshot, springDuration, EaseOut);

        // Phase 3: settle back to original size
        yield return ScaleTo(_originalScale, settleDuration, EaseIn);

        _animating = false;
    }

    IEnumerator ScaleTo(Vector3 target, float duration, System.Func<float, float> easing)
    {
        Vector3 start   = transform.localScale;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            float t = easing(elapsed / duration);
            transform.localScale = Vector3.LerpUnclamped(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = target;
    }

    // Easing functions
    float EaseIn(float t)  => t * t;
    float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}