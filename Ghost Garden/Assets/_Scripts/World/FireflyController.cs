using UnityEngine;

public class FireflyController : MonoBehaviour
{
    [Header("References")]
    public DayNightCycle dayNightCycle;

    [Header("Fade Timing")]
    [Range(0f, 1f)] public float fadeInStart  = 0.72f; 
    [Range(0f, 1f)] public float fadeInEnd    = 0.85f; 
    [Range(0f, 1f)] public float fadeOutStart = 0.18f; 
    [Range(0f, 1f)] public float fadeOutEnd   = 0.28f; 

    [Header("Particle Settings")]
    public Color fireflyColour = new Color(0.6f, 1f, 0.4f, 1f);

    ParticleSystem   _ps;
    ParticleSystem.EmissionModule _emission;
    float _emissionRate; 
    bool  _wasVisible; // Track state to trigger the instant clear

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();

        if (_ps == null)
        {
            Debug.LogError("[FireflyController] No ParticleSystem found.");
            return;
        }

        _emission     = _ps.emission;
        _emissionRate = _emission.rateOverTime.constant;

        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();

        // Initialization
        SetAlpha(0f);
        _emission.rateOverTime = 0f;
        _wasVisible = false;
    }

    void Update()
    {
        if (_ps == null || dayNightCycle == null) return;

        float t     = dayNightCycle.CurrentTime;
        float alpha = ComputeAlpha(t);
        bool isCurrentlyVisible = alpha > 0.001f;

        // THE FIX: Detect the transition from visible to invisible
        if (_wasVisible && !isCurrentlyVisible)
        {
            // Stop emission AND wipe all existing particles immediately
            _ps.Clear(); 
        }

        SetAlpha(alpha);
        _emission.rateOverTime = isCurrentlyVisible ? _emissionRate * alpha : 0f;
        
        _wasVisible = isCurrentlyVisible;
    }

    float ComputeAlpha(float t)
    {
        if (IsBetween(t, fadeInStart, fadeInEnd))
            return Mathf.SmoothStep(0f, 1f, InverseLerpWrapped(fadeInStart, fadeInEnd, t));

        if (IsBetween(t, fadeOutStart, fadeOutEnd))
            return Mathf.SmoothStep(1f, 0f, InverseLerpWrapped(fadeOutStart, fadeOutEnd, t));

        return IsNight(t) ? 1f : 0f;
    }

    void SetAlpha(float alpha)
    {
        var main  = _ps.main;
        Color col = fireflyColour;
        col.a     = alpha;
        main.startColor = col;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    bool IsNight(float t)
    {
        if (fadeInEnd > fadeOutStart) // wraps midnight
            return t >= fadeInEnd || t < fadeOutStart;
        else
            return t >= fadeInEnd && t < fadeOutStart;
    }

    bool IsBetween(float t, float a, float b)
    {
        if (a <= b) return t >= a && t <= b;
        return t >= a || t <= b;
    }

    float InverseLerpWrapped(float a, float b, float value)
    {
        float range = b >= a ? b - a : (1f - a) + b;
        float dist  = value >= a ? value - a : (1f - a) + value;
        if (range < 0.0001f) return 0f;
        return Mathf.Clamp01(dist / range);
    }
}