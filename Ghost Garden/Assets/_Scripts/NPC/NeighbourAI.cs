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

    public bool IsWalking   { get; private set; }
    public bool IsNearHouse => IsWalking && housePosition != null &&
                               Vector3.Distance(transform.position, housePosition.position)
                               <= nearHouseDistance;
    public float WalkElapsed { get; private set; }
    public bool isInspecting;

    NavMeshAgent _agent;
    Animator     _animator;
    bool         _waitingForPath;

    List<Transform> _route = new List<Transform>();
    int _routeIndex;

    void Awake() => Instance = this;

    void Start()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
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
        _waitingForPath = false;
        WalkElapsed     = 0f;

        // Build full round-trip route.
        // Example: waypoints = [A, B, C] → route = [A, B, C, B, A]
        _route.Clear();

        for (int i = 0; i < waypoints.Length; i++)
            _route.Add(waypoints[i]);

        for (int i = waypoints.Length - 2; i >= 0; i--)
            _route.Add(waypoints[i]);

        // Log route for debugging
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
            SetWalking(false);
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
        _waitingForPath = true;
        SetWalking(true);
    }

    // Sets the IsWalking state and keeps the Animator in sync
    void SetWalking(bool walking)
    {
        IsWalking = walking;
        _animator?.SetBool("IsWalking", walking);
    }

    public void NoticeGarden()
    {
        _agent.isStopped = true;
        isInspecting     = true;
        SetWalking(false);
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