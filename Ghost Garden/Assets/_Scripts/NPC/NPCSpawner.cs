using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    public GameObject[] npcPrefabs;

    [Header("Spawn Settings")]
    public int minNPCsPerDay = 2;
    public int maxNPCsPerDay = 5;

    [Header("Path Settings")]
    public Transform pathStart;
    public Transform pathEnd;
    public float lateralJitter = 0.4f;

    [Header("Timing")]
    public float walkTimeMin = 0.25f;
    public float walkTimeMax = 0.75f;
    public float walkSpeedMin = 0.8f;
    public float walkSpeedMax = 1.4f;

    [Header("Waypoint Container")]
    public Transform waypointContainer;

    List<GameObject> _spawnedThisDay = new List<GameObject>();
    List<GameObject> _waypointSets   = new List<GameObject>();

    void Start()
    {
        // Sanity check all references on startup so problems show up immediately
        Debug.Log($"[NPCSpawner] Start — prefabs={(npcPrefabs == null ? "NULL!!!" : npcPrefabs.Length.ToString())}, pathStart={(pathStart == null ? "NULL!!!" : pathStart.name)}, pathEnd={(pathEnd == null ? "NULL!!!" : pathEnd.name)}, waypointContainer={(waypointContainer == null ? "NULL (using self)" : waypointContainer.name)}");

        if (npcPrefabs != null)
        {
            for (int i = 0; i < npcPrefabs.Length; i++)
                Debug.Log($"[NPCSpawner] Prefab[{i}] = {(npcPrefabs[i] == null ? "NULL!!!" : npcPrefabs[i].name)}");
        }
    }

    public void SpawnDailyNPCs()
    {
        Debug.Log("[NPCSpawner] SpawnDailyNPCs called.");

        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("[NPCSpawner] No NPC prefabs assigned! Aborting spawn.");
            return;
        }
        if (pathStart == null || pathEnd == null)
        {
            Debug.LogError("[NPCSpawner] pathStart or pathEnd is not assigned! Aborting spawn.");
            return;
        }

        CleanupPreviousDay();

        int count = Random.Range(minNPCsPerDay, maxNPCsPerDay + 1);
        Debug.Log($"[NPCSpawner] Attempting to spawn {count} NPCs this day.");

        int successCount = 0;
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
            bool startFromA   = Random.value > 0.5f;
            Transform spawnEnd = startFromA ? pathStart : pathEnd;
            Transform destEnd  = startFromA ? pathEnd   : pathStart;

            float xOffset    = Random.Range(-lateralJitter, lateralJitter);
            Vector3 spawnPos = spawnEnd.position + new Vector3(xOffset, 0f, 0f);

            Debug.Log($"[NPCSpawner] NPC {i}: prefab={prefab.name}, spawnPos={spawnPos}, direction={(startFromA ? "A→B" : "B→A")}");

            if (!NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"[NPCSpawner] NPC {i}: No NavMesh found within 2 units of {spawnPos}. Skipping. (Is your NavMesh baked? Is the path over NavMesh?)");
                continue;
            }

            spawnPos = hit.position;
            Debug.Log($"[NPCSpawner] NPC {i}: NavMesh snap succeeded → {spawnPos}");

            Transform wpA = CreateWaypoint($"WP_{i}_A", spawnPos);
            Transform wpB = CreateWaypoint($"WP_{i}_B", destEnd.position + new Vector3(xOffset, 0f, 0f));

            GameObject npcGO = Instantiate(prefab, spawnPos, Quaternion.identity);
            npcGO.name = $"{prefab.name}_Day_{i}";

            BackgroundNPC npc = npcGO.GetComponent<BackgroundNPC>();
            if (npc == null)
            {
                Debug.LogError($"[NPCSpawner] NPC {i}: Prefab '{prefab.name}' has no BackgroundNPC component! Destroying.");
                Destroy(npcGO);
                continue;
            }

            npc.waypoints     = new Transform[] { wpA, wpB };
            npc.walkStartTime = Random.Range(walkTimeMin, walkTimeMax);
            npc.walkSpeed     = Random.Range(walkSpeedMin, walkSpeedMax);

            Debug.Log($"[NPCSpawner] NPC {i}: Spawned '{npcGO.name}', walkStartTime={npc.walkStartTime:F3}, walkSpeed={npc.walkSpeed:F2}");

            _spawnedThisDay.Add(npcGO);
            successCount++;
        }

        Debug.Log($"[NPCSpawner] Done. {successCount}/{count} NPCs spawned successfully. BackgroundNPC.All count={BackgroundNPC.All.Count}");
    }

    public void CleanupPreviousDay()
    {
        Debug.Log($"[NPCSpawner] CleanupPreviousDay — destroying {_spawnedThisDay.Count} NPCs and {_waypointSets.Count} waypoints.");
        foreach (var go in _spawnedThisDay)
            if (go != null) Destroy(go);
        _spawnedThisDay.Clear();

        foreach (var wp in _waypointSets)
            if (wp != null) Destroy(wp);
        _waypointSets.Clear();
    }

    Transform CreateWaypoint(string wpName, Vector3 position)
    {
        GameObject wp = new GameObject(wpName);
        wp.transform.position = position;
        wp.transform.SetParent(waypointContainer != null ? waypointContainer : transform);
        _waypointSets.Add(wp);
        return wp.transform;
    }

    void OnDrawGizmos()
    {
        if (pathStart == null || pathEnd == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pathStart.position, pathEnd.position);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 jitterA = new Vector3(lateralJitter, 0f, 0f);
        Gizmos.DrawLine(pathStart.position - jitterA, pathEnd.position - jitterA);
        Gizmos.DrawLine(pathStart.position + jitterA, pathEnd.position + jitterA);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pathStart.position, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pathEnd.position, 0.3f);
    }
}