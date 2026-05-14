using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    public float dayLengthSeconds     = 120f;
    public float nightSpeedMultiplier = 2f;

    [Range(0f, 1f)] public float nightStart = 0.75f; // dusk
    [Range(0f, 1f)] public float nightEnd   = 0.25f; // dawn

    public float neighbourWalkTime = 0.3f;

    [Header("Lighting")]
    public Light    directionalLight;
    public Gradient skyColour;

    [Header("Skybox")]
    // Drag the Blend material here — it has _CubemapTransition which we drive.
    // In Lighting Settings, also assign this material as the skybox.
    public Material skyboxBlendMaterial;

    // How much of the day/night range is used for transitioning the skybox.
    // 0.1 means the blend happens over 10% of the day cycle around dusk/dawn.
    [Range(0.01f, 0.3f)]
    public float transitionWidth = 0.08f;

    float _time;
    bool  _neighbourTriggeredToday;

    void Start()
    {
        // Make sure the skybox is assigned in Lighting Settings at startup
        if (skyboxBlendMaterial != null)
            RenderSettings.skybox = skyboxBlendMaterial;
    }

    void Update()
    {
        bool  isNight   = IsNightTime(_time);
        float speedMult = isNight ? nightSpeedMultiplier : 1f;
        _time += (Time.deltaTime / dayLengthSeconds) * speedMult;

        // Rotate and tint the sun
        if (directionalLight != null)
        {
            directionalLight.color = skyColour.Evaluate(_time);
            directionalLight.transform.rotation =
                Quaternion.Euler(_time * 360f - 90f, 170f, 0f);
        }

        // Update skybox blend
        UpdateSkybox();

        // Trigger neighbour walk
        if (!_neighbourTriggeredToday && _time >= neighbourWalkTime)
        {
            _neighbourTriggeredToday = true;
            NeighbourAI.Instance?.TriggerDailyWalk();
        }

        // End of day
        if (_time >= 1f)
        {
            _time = 0f;
            _neighbourTriggeredToday = false;
            GameManager.Instance?.AdvanceDay();
        }
    }

    void UpdateSkybox()
    {
        if (skyboxBlendMaterial == null) return;

        // _CubemapTransition: 0 = fully day cubemap, 1 = fully night cubemap.
        // We ramp it up around dusk (nightStart) and back down around dawn (nightEnd).
        float blend = ComputeSkyboxBlend(_time);
        skyboxBlendMaterial.SetFloat("_CubemapTransition", blend);

        // Tell Unity to update ambient lighting to match the new skybox
        DynamicGI.UpdateEnvironment();
    }

    // Returns 0 (full day) → 1 (full night) based on current _time.
    // Smoothly transitions over transitionWidth around dusk and dawn.
    float ComputeSkyboxBlend(float t)
    {
        // Dusk transition: ramp from 0 to 1 as t goes from nightStart to nightStart+width
        float duskStart  = nightStart;
        float duskEnd    = Wrap(nightStart + transitionWidth);

        // Dawn transition: ramp from 1 to 0 as t goes from nightEnd-width to nightEnd
        float dawnStart  = Wrap(nightEnd - transitionWidth);
        float dawnEnd    = nightEnd;

        // Check if we're in the dusk ramp
        float duskT = InverseLerpWrapped(duskStart, duskEnd, t);
        if (duskT >= 0f && duskT <= 1f)
            return Mathf.SmoothStep(0f, 1f, duskT);

        // Check if we're in the dawn ramp
        float dawnT = InverseLerpWrapped(dawnStart, dawnEnd, t);
        if (dawnT >= 0f && dawnT <= 1f)
            return Mathf.SmoothStep(1f, 0f, dawnT);

        // Full night or full day
        return IsNightTime(t) ? 1f : 0f;
    }

    // Inverse lerp that handles values wrapping around 0/1
    float InverseLerpWrapped(float a, float b, float value)
    {
        // Normalise all values relative to a
        float bRel = Wrap(b - a);
        float vRel = Wrap(value - a);

        if (bRel < 0.001f) return -1f; // a == b, undefined
        float t = vRel / bRel;
        return (t >= 0f && t <= 1f) ? t : -1f;
    }

    float Wrap(float t)
    {
        t %= 1f;
        if (t < 0f) t += 1f;
        return t;
    }

    bool IsNightTime(float t)
    {
        if (nightStart < nightEnd)
            return t >= nightStart && t < nightEnd;
        else
            return t >= nightStart || t < nightEnd;
    }

    // Draw a timeline in the Scene view showing day/night/transition zones
    void OnDrawGizmosSelected()
    {
        // Not a 3D gizmo — just useful as a reminder that gizmos are available
    }
}