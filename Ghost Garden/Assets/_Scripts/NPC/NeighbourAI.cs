using UnityEngine;
using UnityEngine.AI;

public class NeighbourAI : MonoBehaviour
{
    public static NeighbourAI Instance;
    public Transform[] waypoints;
    public bool isInspecting;

    NavMeshAgent _agent;
    int _waypointIndex;

    void Awake() => Instance = this;

    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        gameObject.SetActive(false);
    }

    public void TriggerDailyWalk()
    {
        gameObject.SetActive(true);
        _waypointIndex = 0;
        MoveTo(waypoints[0]);
    }

    void Update()
    {
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _waypointIndex++;
            if (_waypointIndex >= waypoints.Length)
            {
                gameObject.SetActive(false);
                return;
            }
            MoveTo(waypoints[_waypointIndex]);
        }
    }

    void MoveTo(Transform t) => _agent.SetDestination(t.position);

    public void NoticeGarden()
    {
        _agent.isStopped = true;
        isInspecting = true;
        Invoke(nameof(WalkToGarden), 2f);
    }

    void WalkToGarden()
    {
        GameManager.Instance?.TriggerWin();
    }
}