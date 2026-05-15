using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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

    [Header("Win Sequence")]
    [Tooltip("Empty GameObject placed at the watering can's location — neighbour walks here to pick it up")]
    public Transform wateringCanWaypoint;
    [Tooltip("The WateringCup scene object that will be reparented to the hand")]
    public GameObject wateringCanObject;
    [Tooltip("The hand bone Transform on the neighbour's skeleton (e.g. RightHand)")]
    public Transform handBone;
    [Tooltip("Local position offset so the can sits correctly in the hand — tweak in Play Mode then copy here")]
    public Vector3 wateringCanHandOffset = Vector3.zero;
    [Tooltip("Local rotation offset so the can faces the right way — tweak in Play Mode then copy here")]
    public Vector3 wateringCanHandRotation = Vector3.zero;
    [Tooltip("The original world scale of the watering can before it was reparented")]
    public Vector3 wateringCanWorldScale = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("Exact length in seconds of your Watering animation clip")]
    public float wateringAnimationLength = 3f;

    public bool IsWalking    { get; private set; }
    public bool IsNearHouse  => IsWalking && housePosition != null &&
                                Vector3.Distance(transform.position, housePosition.position)
                                <= nearHouseDistance;
    public float WalkElapsed { get; private set; }
    public bool isInspecting;

    NavMeshAgent    _agent;
    Animator        _animator;
    bool            _waitingForPath;
    bool            _inWinSequence;

    List<Transform> _route = new List<Transform>();
    int             _routeIndex;

    void Awake() => Instance = this;

    void Start()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (handBone == null)
        {
            handBone = FindBoneByName(transform, "RightHand");
            if (handBone == null)
                Debug.LogWarning("[NeighbourAI] Hand bone not found! Check the bone name.");
            else
                Debug.Log($"[NeighbourAI] Hand bone auto-assigned: {handBone.name}");
        }

        gameObject.SetActive(false);
    }

    Transform FindBoneByName(Transform root, string boneName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == boneName) return t;
        return null;
    }

    // ─── Daily Walk ───────────────────────────────────────────────────────────

    public void TriggerDailyWalk()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("[NeighbourAI] No waypoints assigned!");
            return;
        }

        isInspecting    = false;
        _inWinSequence  = false;
        _waitingForPath = false;
        WalkElapsed     = 0f;

        _route.Clear();
        for (int i = 0; i < waypoints.Length; i++)
            _route.Add(waypoints[i]);
        for (int i = waypoints.Length - 2; i >= 0; i--)
            _route.Add(waypoints[i]);

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
        if (_inWinSequence) return;

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

    void SetWalking(bool walking)
    {
        IsWalking = walking;
        _animator?.SetBool("IsWalking", walking);
    }

    // ─── Notice Garden → Win Sequence ────────────────────────────────────────

    public void NoticeGarden()
    {
        _inWinSequence   = true;
        isInspecting     = true;

        _agent.ResetPath();
        _agent.isStopped = true;
        SetWalking(false);
        AudioManager.Instance?.StopFootsteps();
        Debug.Log("[NeighbourAI] Neighbour noticed the garden!");

        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Stand and look for 2 seconds
        yield return new WaitForSeconds(2f);

        // 2. Walk to the watering can
        if (wateringCanWaypoint != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(wateringCanWaypoint.position);
            SetWalking(true);
            AudioManager.Instance?.StartFootsteps();

            yield return new WaitUntil(() => !_agent.pathPending);
            yield return new WaitUntil(() =>
                _agent.remainingDistance <= _agent.stoppingDistance);

            SetWalking(false);
            AudioManager.Instance?.StopFootsteps();
            Debug.Log("[NeighbourAI] Arrived at watering can.");
        }
        else
        {
            Debug.LogWarning("[NeighbourAI] wateringCanWaypoint not assigned — skipping walk to can.");
        }

        // 3. Snap the watering can into the hand bone
        if (wateringCanObject != null && handBone != null)
        {
            // Kill physics so the Rigidbody doesn't fight the hand transform
            Rigidbody canRb = wateringCanObject.GetComponent<Rigidbody>();
            if (canRb != null)
            {
                canRb.isKinematic = true;
                canRb.useGravity  = false;
            }

            // false = snap into local space, don't preserve world position
            wateringCanObject.transform.SetParent(handBone, false);
            wateringCanObject.transform.localPosition = wateringCanHandOffset;
            wateringCanObject.transform.localRotation = Quaternion.Euler(wateringCanHandRotation);

            // Counteract the hand bone's inherited scale while preserving the can's original size
            Vector3 parentScale = handBone.lossyScale;
            wateringCanObject.transform.localScale = new Vector3(
                wateringCanWorldScale.x / parentScale.x,
                wateringCanWorldScale.y / parentScale.y,
                wateringCanWorldScale.z / parentScale.z
            );

            Debug.Log("[NeighbourAI] Watering can attached to hand.");
        }
        else
        {
            Debug.LogWarning("[NeighbourAI] wateringCanObject or handBone not assigned — can won't attach.");
        }

        // 4. Play the watering animation
        _animator?.SetTrigger("DoWatering");
        Debug.Log("[NeighbourAI] Playing watering animation.");

        // 5. Wait for the animation to finish
        yield return new WaitForSeconds(wateringAnimationLength);

        // 6. Trigger the win screen
        Debug.Log("[NeighbourAI] Win sequence complete!");
        GameManager.Instance?.TriggerWin();
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (housePosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(housePosition.position, nearHouseDistance);
    }
}