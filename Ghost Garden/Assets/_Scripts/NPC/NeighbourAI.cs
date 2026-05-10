using UnityEngine;
using UnityEngine.AI;

public class NeighbourAI : MonoBehaviour
{
    public static NeighbourAI Instance;

    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Proximity Detection")]
    public Transform housePosition;
    public float nearHouseDistance = 6f;

    public bool IsWalking   { get; private set; }
    public bool IsNearHouse => IsWalking && housePosition != null &&
                               Vector3.Distance(transform.position, housePosition.position)
                               <= nearHouseDistance;
    public bool isInspecting;

    NavMeshAgent _agent;
    int  _waypointIndex;
    bool _waitingForPath;

    void Awake() => Instance = this;

    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        gameObject.SetActive(false);
    }

    public void TriggerDailyWalk()
    {
        isInspecting     = false;
        _waypointIndex   = 0;
        IsWalking        = false;
        _waitingForPath  = false;
        gameObject.SetActive(true);
        AudioManager.Instance?.StartFootsteps();
        MoveTo(waypoints[0]);
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
                AudioManager.Instance?.StopFootsteps();
                gameObject.SetActive(false);
                return;
            }

            MoveTo(waypoints[_waypointIndex]);
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

    void WalkToGarden()
    {
        GameManager.Instance?.TriggerWin();
    }

    void OnDrawGizmosSelected()
    {
        if (housePosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(housePosition.position, nearHouseDistance);
    }
}