using UnityEngine;
using System.Collections;

// Attach to any decorative nudgeable object (Role = None or Foliage).
// Wire NudgeableObject's OnNudged event → NudgeSquishAnimation.Squish()
//
// For foliage (trees/flowers), set isFoliage = true in the Inspector.
// This swaps the squish for a gentle sway that looks more natural on plants.

public class NudgeSquishAnimation : MonoBehaviour
{
    [Header("Type")]
    [Tooltip("Enable for trees and flowers — plays a sway instead of a squish")]
    public bool isFoliage = false;

    [Header("Squish Settings (non-foliage)")]
    public float squishScaleY    = 0.5f;
    public float squishScaleXZ   = 1.3f;
    public float squishDuration  = 0.08f;
    public float springDuration  = 0.18f;
    public float overshootAmount = 0.12f;
    public float settleDuration  = 0.1f;

    [Header("Foliage Sway Settings")]
    public float swayAngle    = 12f;   // max lean angle in degrees
    public float swayDuration = 0.8f;  // total sway time
    public float swaySpeed    = 3f;    // oscillations per second

    Vector3 _originalScale;
    bool    _animating;

    void Start()
    {
        _originalScale = transform.localScale;
    }

    // Called from NudgeableObject's OnNudged UnityEvent
    public void Squish()
    {
        if (_animating) return;
        StartCoroutine(isFoliage ? FoliageSway() : SquishRoutine());
    }

    // ── Squish (props, pots, decorative objects) ──────────────────────────────

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

        yield return ScaleTo(squished, squishDuration, EaseIn);
        yield return ScaleTo(overshot, springDuration, EaseOut);
        yield return ScaleTo(_originalScale, settleDuration, EaseIn);

        _animating = false;
    }

    // ── Foliage sway (trees, flowers, bushes) ─────────────────────────────────

    IEnumerator FoliageSway()
    {
        _animating = true;

        Quaternion startRot = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < swayDuration)
        {
            // Amplitude fades out so it settles back to rest
            float amplitude = swayAngle * (1f - elapsed / swayDuration);
            float angle     = Mathf.Sin(elapsed * swaySpeed * Mathf.PI) * amplitude;
            transform.localRotation = startRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        _animating = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    IEnumerator ScaleTo(Vector3 target, float duration, System.Func<float, float> easing)
    {
        Vector3 start   = transform.localScale;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.LerpUnclamped(start, target,
                                       easing(elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = target;
    }

    float EaseIn(float t)  => t * t;
    float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}