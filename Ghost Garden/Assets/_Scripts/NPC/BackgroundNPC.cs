using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BackgroundNPC : MonoBehaviour
{
    public static List<BackgroundNPC> All = new List<BackgroundNPC>();

    [Header("Waypoints")]
    // Place in forward order e.g. [Start, Mid, End]
    // The script will automatically walk them in reverse to get back to start.
    public Transform[] waypoints;

    [Header("Timing")]
    [Range(0f, 1f)]
    [Tooltip("Time of day (0–1) when this NPC begins their walk, matching DayNightCycle's CurrentTime.")]
    public float walkStartTime = 0.3f;
    [Tooltip("How fast this NPC walks relative to its NavMeshAgent speed.")]
    public float walkSpeed = 1f;

    [Header("Audio")]
    [Tooltip("Optional: give each NPC their own footstep instance. Leave unchecked to skip footstep audio.")]
    public bool playFootstepAudio = true;
    [Tooltip("FMOD event path for this NPC's mumbling voice e.g. event:/NPC Mumble. Leave empty for no voice.")]
    public string voiceEventPath = "";

    public bool IsWalking { get; private set; }

    NavMeshAgent    _agent;
    Animator        _animator;
    bool            _waitingForPath;
    bool            _triggeredToday;

    List<Transform> _route = new List<Transform>();
    int             _routeIndex;

    FMOD.Studio.EventInstance _footstepsInstance;
    FMOD.Studio.EventInstance _voiceInstance;
    bool _footstepsPlaying;
    bool _voicePlaying;
    bool _hasVoice;

    void Awake()
    {
        All.Add(this);
    }

    void OnDestroy()
    {
        All.Remove(this);

        if (playFootstepAudio)
        {
            _footstepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _footstepsInstance.release();
        }

        if (_hasVoice)
        {
            _voiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _voiceInstance.release();
        }
    }

    void Start()
    {
        _agent       = GetComponent<NavMeshAgent>();
        _agent.speed = walkSpeed;
        _animator    = GetComponent<Animator>();

        if (playFootstepAudio)
        {
            _footstepsInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Footsteps");
            _footstepsInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            _footstepsInstance.setParameterByName("Walk Surface", 0f);
        }

        _hasVoice = !string.IsNullOrEmpty(voiceEventPath);
        if (_hasVoice)
        {
            _voiceInstance = FMODUnity.RuntimeManager.CreateInstance(voiceEventPath);
            _voiceInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        // Check if it's time to start the walk today
        DayNightCycle cycle = DayNightCycle.FindAnyObjectByType<DayNightCycle>();
        if (!_triggeredToday && cycle != null && cycle.CurrentTime >= walkStartTime)
        {
            _triggeredToday = true;
            TriggerWalk();
        }

        if (!IsWalking) return;

        if (playFootstepAudio)
            _footstepsInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        if (_hasVoice)
            _voiceInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

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
                // Walk complete — hide until tomorrow
                StopFootsteps();
                StopVoice();
                gameObject.SetActive(false);
                return;
            }

            MoveTo(_route[_routeIndex]);
        }
    }

    // Called by GameManager when a new day starts
    public void ResetDay()
    {
        _triggeredToday = false;
    }

    void TriggerWalk()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[BackgroundNPC] {gameObject.name} has no waypoints assigned!");
            return;
        }

        _route.Clear();
        for (int i = 0; i < waypoints.Length; i++)
            _route.Add(waypoints[i]);
        for (int i = waypoints.Length - 2; i >= 0; i--)
            _route.Add(waypoints[i]);

        _routeIndex = 0;
        gameObject.SetActive(true);
        StartFootsteps();
        StartVoice();
        MoveTo(_route[_routeIndex]);
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

    void StartFootsteps()
    {
        if (!playFootstepAudio || _footstepsPlaying) return;
        _footstepsInstance.start();
        _footstepsPlaying = true;
    }

    void StopFootsteps()
    {
        if (!playFootstepAudio || !_footstepsPlaying) return;
        _footstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _footstepsPlaying = false;
    }

    void StartVoice()
    {
        if (!_hasVoice || _voicePlaying) return;
        _voiceInstance.start();
        _voicePlaying = true;
    }

    void StopVoice()
    {
        if (!_hasVoice || !_voicePlaying) return;
        _voiceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _voicePlaying = false;
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}