using UnityEngine;
using UnityEngine.AI;

public class NeighbourAI : MonoBehaviour
{
    public static NeighbourAI Instance;

    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Proximity Detection")]
    // Set this in the Inspector to the position of your house/windchimes
    public Transform housePosition;
    public float nearHouseDistance = 6f;

    // Read by WinSequenceManager
    public bool IsWalking  { get; private set; }
    public bool IsNearHouse => IsWalking && housePosition != null &&
                               Vector3.Distance(transform.position, housePosition.position)
                               <= nearHouseDistance;

    public bool isInspecting;

    NavMeshAgent _agent;
    int _waypointIndex;
    bool _waitingForPath;

    void Awake() => Instance = this;

    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        gameObject.SetActive(false);
    }

    public void TriggerDailyWalk()
    {
        isInspecting = false;
        _waypointIndex = 0;
        IsWalking = false;
        _waitingForPath = false;
        gameObject.SetActive(true);
        MoveTo(waypoints[0]);
        Debug.Log("[NeighbourAI] Daily walk started.");
    }

    void Update()
    {
        if (!IsWalking) return;

        if (_waitingForPath)
        {
            if (_agent.pathPending) return;
            _waitingForPath = false;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            IsWalking = false;
            _waypointIndex++;

            if (_waypointIndex >= waypoints.Length)
            {
                Debug.Log("[NeighbourAI] Walk complete — neighbour left.");
                gameObject.SetActive(false);
                return;
            }

            MoveTo(waypoints[_waypointIndex]);
        }
    }

    void MoveTo(Transform t)
    {
        _agent.SetDestination(t.position);
        IsWalking = true;
        _waitingForPath = true;
    }

    public void NoticeGarden()
    {
        _agent.isStopped = true;
        IsWalking = false;
        isInspecting = true;
        Debug.Log("[NeighbourAI] Neighbour noticed the garden!");
        // Play a reaction animation here if you have one
        Invoke(nameof(WalkToGarden), 2f);
    }

    void WalkToGarden()
    {
        GameManager.Instance?.TriggerWin();
    }

    // Draw the near-house detection radius in the Scene view for easy tuning
    void OnDrawGizmosSelected()
    {
        if (housePosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(housePosition.position, nearHouseDistance);
    }
}