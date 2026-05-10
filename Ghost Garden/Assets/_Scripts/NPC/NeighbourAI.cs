using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NeighbourAI : MonoBehaviour
{
    public static NeighbourAI Instance;

    [Header("Waypoints")]
    // Place in forward order only e.g. [Home, Mid, FarEnd]
    // The script will automatically walk them in reverse to get back home.
    public Transform[] waypoints;

    [Header("Proximity Detection")]
    public Transform housePosition;
    public float nearHouseDistance = 6f;

    public bool IsWalking  { get; private set; }
    public bool IsNearHouse => IsWalking && housePosition != null &&
                               Vector3.Distance(transform.position, housePosition.position)
                               <= nearHouseDistance;
    public float WalkElapsed { get; private set; }
    public bool isInspecting;

    NavMeshAgent _agent;
    bool _waitingForPath;

    List<Transform> _route = new List<Transform>();
    int _routeIndex;

    void Awake() => Instance = this;

    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        gameObject.SetActive(false);
    }

    public void TriggerDailyWalk()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("[NeighbourAI] No waypoints assigned!");
            return;
        }

        isInspecting    = false;
        IsWalking       = false;
        _waitingForPath = false;
        WalkElapsed     = 0f;

        // Build the full route as a List so the logic is easy to read and verify.
        // Example: waypoints = [A, B, C]
        // Forward pass:  A → B → C
        // Reverse pass:  B → A         (skip C since we're already there)
        // Full route:    [A, B, C, B, A]
        _route.Clear();

        // Forward
        for (int i = 0; i < waypoints.Length; i++)
            _route.Add(waypoints[i]);

        // Reverse — start from second-to-last so we don't repeat the turnaround point
        for (int i = waypoints.Length - 2; i >= 0; i--)
            _route.Add(waypoints[i]);

        // Log the built route so you can verify it in the Console
        string routeStr = "";
        for (int i = 0; i < _route.Count; i++)
            routeStr += _route[i].name + (i < _route.Count - 1 ? " → " : "");
        Debug.Log($"[NeighbourAI] Daily walk route: {routeStr}");

        _routeIndex = 0;
        gameObject.SetActive(true);
        AudioManager.Instance?.StartFootsteps();
        MoveTo(_route[_routeIndex]);
    }

    void Update()
    {
        if (IsWalking)
        {
            WalkElapsed += Time.deltaTime;
            AudioManager.Instance?.UpdateNeighbourFootstepPosition(transform.position);
        }

        if (!IsWalking) return;

        if (_waitingForPath)
        {
            if (_agent.pathPending) return;
            _waitingForPath = false;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            IsWalking = false;
            _routeIndex++;

            if (_routeIndex >= _route.Count)
            {
                // Back home — end walk for today
                Debug.Log("[NeighbourAI] Walk complete, heading home.");
                AudioManager.Instance?.StopFootsteps();
                gameObject.SetActive(false);
                return;
            }

            Debug.Log($"[NeighbourAI] Moving to route stop {_routeIndex}: {_route[_routeIndex].name}");
            MoveTo(_route[_routeIndex]);
        }
    }

    void MoveTo(Transform t)
    {
        _agent.SetDestination(t.position);
        IsWalking       = true;
        _waitingForPath = true;
    }

    public void NoticeGarden()
    {
        _agent.isStopped = true;
        IsWalking        = false;
        isInspecting     = true;
        AudioManager.Instance?.StopFootsteps();
        Debug.Log("[NeighbourAI] Neighbour noticed the garden!");
        Invoke(nameof(WalkToGarden), 2f);
    }

    void WalkToGarden() => GameManager.Instance?.TriggerWin();

    void OnDrawGizmosSelected()
    {
        if (housePosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(housePosition.position, nearHouseDistance);
    }
}