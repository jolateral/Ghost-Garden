using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    public float dayLengthSeconds = 120f; // 2 real minutes = 1 game day
    public float neighbourWalkTime = 0.2f; // time of day the neighbour starts walking (0=midnight, 0.25=6am, 0.5=noon)

    [Header("Lighting")]
    public Light directionalLight;
    public Gradient skyColour;

    float _time; // 0 to 1 within a single day
    bool _neighbourTriggeredToday; // so we only trigger the walk once per day

    void Update()
    {
        _time += Time.deltaTime / dayLengthSeconds;

        // Rotate and tint the sun
        if (directionalLight != null)
        {
            directionalLight.color = skyColour.Evaluate(_time);
            directionalLight.transform.rotation =
                Quaternion.Euler(_time * 360f - 90f, 170f, 0f);
        }

        // Trigger the neighbour's walk at the configured time of day
        if (!_neighbourTriggeredToday && _time >= neighbourWalkTime)
        {
            _neighbourTriggeredToday = true;
            NeighbourAI.Instance?.TriggerDailyWalk();
        }

        // End of day
        if (_time >= 1f)
        {
            _time = 0f;
            _neighbourTriggeredToday = false; // reset for next day
            GameManager.Instance?.AdvanceDay();
        }
    }
}