using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    public float dayLengthSeconds = 120f; // 2 real minutes = 1 game day

    [Header("Lighting")]
    public Light directionalLight;
    public Gradient skyColour;

    float _time; // 0 to 1 within a single day

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

        if (_time >= 1f)
        {
            _time = 0f;
            GameManager.Instance?.AdvanceDay();
            NeighbourAI.Instance?.TriggerDailyWalk();
        }
    }
}