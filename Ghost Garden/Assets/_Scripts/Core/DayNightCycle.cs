using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    public float dayLengthSeconds  = 120f; // how long the DAY portion lasts in real seconds
    public float nightSpeedMultiplier = 2f; // how much faster night passes vs day

    // _time ranges 0-1. Define which portion counts as "night" (faster).
    // 0.0  = midnight
    // 0.25 = 6am  (dawn)
    // 0.5  = noon
    // 0.75 = 6pm  (dusk)
    // 1.0  = midnight again
    [Range(0f, 1f)] public float nightStart = 0.75f; // dusk — night speed kicks in
    [Range(0f, 1f)] public float nightEnd   = 0.25f; // dawn — day speed resumes

    public float neighbourWalkTime = 0.3f;

    [Header("Lighting")]
    public Light directionalLight;
    public Gradient skyColour;

    float _time;
    bool _neighbourTriggeredToday;

    void Update()
    {
        // Work out if we're currently in the night portion
        bool isNight = IsNightTime(_time);
        float speedMult = isNight ? nightSpeedMultiplier : 1f;

        // Advance time — day portion uses dayLengthSeconds as the base,
        // night passes at nightSpeedMultiplier times that rate
        _time += (Time.deltaTime / dayLengthSeconds) * speedMult;

        // Rotate and tint the sun
        if (directionalLight != null)
        {
            directionalLight.color = skyColour.Evaluate(_time);
            directionalLight.transform.rotation =
                Quaternion.Euler(_time * 360f - 90f, 170f, 0f);
        }

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

    // Returns true if _time falls within the night window.
    // Handles the midnight wrap-around (nightStart > nightEnd).
    bool IsNightTime(float t)
    {
        if (nightStart < nightEnd)
        {
            // Simple case: night is a contiguous window e.g. 0.2 to 0.4
            return t >= nightStart && t < nightEnd;
        }
        else
        {
            // Wrap-around case: night crosses midnight e.g. 0.75 to 0.25
            return t >= nightStart || t < nightEnd;
        }
    }
}