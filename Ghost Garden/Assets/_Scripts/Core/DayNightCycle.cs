using UnityEngine;
public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    public float dayLengthSeconds     = 120f;
    public float nightSpeedMultiplier = 2f;
    [Range(0f, 1f)] public float nightStart = 0.75f;
    [Range(0f, 1f)] public float nightEnd   = 0.25f;
    public float neighbourWalkTime = 0.3f;

    [Header("Lighting")]
    public Light    directionalLight;
    public Gradient skyColour;

    [Header("Skybox")]
    public Material skyboxBlendMaterial;
    [Range(0.01f, 0.3f)]
    public float transitionWidth = 0.08f;

    [Header("NPC Spawning")]
    [Tooltip("Drag your NPCSpawner GameObject here.")]
    public NPCSpawner npcSpawner;

    float _time;
    bool  _neighbourTriggeredToday;
    bool  _npcsSpawnedToday;

    public float CurrentTime => _time;

    void Start()
    {
        if (skyboxBlendMaterial != null)
            RenderSettings.skybox = skyboxBlendMaterial;

        Debug.Log($"[DayNightCycle] Start — _time={_time:F3}, isNight={IsNightTime(_time)}, npcSpawner={(npcSpawner == null ? "NULL !!!" : npcSpawner.name)}");

        if (!IsNightTime(_time))
        {
            Debug.Log("[DayNightCycle] Daytime on Start — calling SpawnDailyNPCs now.");
            npcSpawner?.SpawnDailyNPCs();
            _npcsSpawnedToday = true;
        }
        else
        {
            Debug.Log("[DayNightCycle] Nighttime on Start — waiting for dawn to spawn NPCs.");
        }
    }

    void Update()
    {
        bool  isNight   = IsNightTime(_time);
        float speedMult = isNight ? nightSpeedMultiplier : 1f;
        _time += (Time.deltaTime / dayLengthSeconds) * speedMult;

        if (directionalLight != null)
        {
            directionalLight.color = skyColour.Evaluate(_time);
            directionalLight.transform.rotation =
                Quaternion.Euler(_time * 360f - 90f, 170f, 0f);
        }

        UpdateSkybox();
        AudioManager.Instance?.SetTimeOfDay(isNight);

        if (!_neighbourTriggeredToday && _time >= neighbourWalkTime)
        {
            _neighbourTriggeredToday = true;
            NeighbourAI.Instance?.TriggerDailyWalk();
        }

        // Dawn spawn trigger
        if (!isNight && !_npcsSpawnedToday)
        {
            Debug.Log($"[DayNightCycle] Dawn at _time={_time:F3} — calling SpawnDailyNPCs.");
            npcSpawner?.SpawnDailyNPCs();
            _npcsSpawnedToday = true;
        }

        foreach (var npc in BackgroundNPC.All)
        {
            if (!npc.TriggeredToday && !IsNightTime(_time) && _time >= npc.walkStartTime)
            {
                Debug.Log($"[DayNightCycle] Triggering {npc.gameObject.name} at _time={_time:F3} (walkStartTime={npc.walkStartTime:F3})");
                npc.TriggerWalk();
            }
        }

        if (_time >= 1f)
        {
            Debug.Log("[DayNightCycle] Midnight rollover — resetting day.");
            _time = 0f;
            _neighbourTriggeredToday = false;
            _npcsSpawnedToday        = false;
            GameManager.Instance?.AdvanceDay();
        }
    }

    void UpdateSkybox()
    {
        if (skyboxBlendMaterial == null) return;
        float blend = ComputeSkyboxBlend(_time);
        skyboxBlendMaterial.SetFloat("_CubemapTransition", blend);
        DynamicGI.UpdateEnvironment();
    }

    float ComputeSkyboxBlend(float t)
    {
        float duskStart = nightStart;
        float duskEnd   = Wrap(nightStart + transitionWidth);
        float dawnStart = Wrap(nightEnd - transitionWidth);
        float dawnEnd   = nightEnd;

        float duskT = InverseLerpWrapped(duskStart, duskEnd, t);
        if (duskT >= 0f && duskT <= 1f)
            return Mathf.SmoothStep(0f, 1f, duskT);

        float dawnT = InverseLerpWrapped(dawnStart, dawnEnd, t);
        if (dawnT >= 0f && dawnT <= 1f)
            return Mathf.SmoothStep(1f, 0f, dawnT);

        return IsNightTime(t) ? 1f : 0f;
    }

    float InverseLerpWrapped(float a, float b, float value)
    {
        float bRel = Wrap(b - a);
        float vRel = Wrap(value - a);
        if (bRel < 0.001f) return -1f;
        float t = vRel / bRel;
        return (t >= 0f && t <= 1f) ? t : -1f;
    }

    float Wrap(float t)
    {
        t %= 1f;
        if (t < 0f) t += 1f;
        return t;
    }

    public bool IsNightTime(float t)
    {
        if (nightStart < nightEnd)
            return t >= nightStart && t < nightEnd;
        else
            return t >= nightStart || t < nightEnd;
    }
}