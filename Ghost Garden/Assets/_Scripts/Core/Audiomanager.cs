using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    const string EVT_BIRD_CHIRP       = "event:/Bird Chirp";
    const string EVT_FOOTSTEPS        = "event:/Footsteps";
    const string EVT_AMBIENCE         = "event:/Level Ambience";
    const string EVT_MUSIC            = "event:/Music Loop";
    const string EVT_NUDGE            = "event:/Nudge";
    const string EVT_PAUSE_TRANSITION = "event:/Pause Transition";
    const string EVT_UI_POP           = "event:/UI Pop";
    const string EVT_WATERING_CAN     = "event:/Watering Can + Shelf";
    const string EVT_WINDCHIME        = "event:/Windchime";

    EventInstance _musicInstance;
    EventInstance _ambienceInstance;
    EventInstance _playerFootstepsInstance;
    EventInstance _neighbourFootstepsInstance;

    bool _playerFootstepsPlaying;
    bool _neighbourFootstepsPlaying;

    // Set true in Inspector to see footstep debug logs in Console
    [Header("Debug")]
    public bool debugFootsteps = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _musicInstance    = RuntimeManager.CreateInstance(EVT_MUSIC);
        _ambienceInstance = RuntimeManager.CreateInstance(EVT_AMBIENCE);
        _musicInstance.start();
        _ambienceInstance.start();

        // Create footstep instances and verify they loaded correctly
        _playerFootstepsInstance    = RuntimeManager.CreateInstance(EVT_FOOTSTEPS);
        _neighbourFootstepsInstance = RuntimeManager.CreateInstance(EVT_FOOTSTEPS);

        // Check if the instances are valid
        _playerFootstepsInstance.getDescription(out EventDescription desc);
        desc.isValid();
        if (debugFootsteps)
        {
            desc.getPath(out string path);
            Debug.Log($"[AudioManager] Player footstep event loaded: '{path}'. Instance valid: {_playerFootstepsInstance.isValid()}");
        }

        _playerFootstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
        _neighbourFootstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
        _playerFootstepsInstance.setParameterByName("Walk Surface", 0f);
        _neighbourFootstepsInstance.setParameterByName("Walk Surface", 0f);
    }

    void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();
        _ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _ambienceInstance.release();
        _playerFootstepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _playerFootstepsInstance.release();
        _neighbourFootstepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _neighbourFootstepsInstance.release();
    }

    public void SetPlayerFootstepsActive(bool isMoving, Vector3 worldPos)
    {
        if (!_playerFootstepsInstance.isValid())
        {
            Debug.LogError("[AudioManager] Player footstep instance is NOT valid! The FMOD event may not have loaded.");
            return;
        }

        _playerFootstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));

        if (isMoving && !_playerFootstepsPlaying)
        {
            FMOD.RESULT result = _playerFootstepsInstance.start();
            _playerFootstepsPlaying = true;
            if (debugFootsteps)
                Debug.Log($"[AudioManager] Player footsteps START — FMOD result: {result}");
        }
        else if (!isMoving && _playerFootstepsPlaying)
        {
            _playerFootstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _playerFootstepsPlaying = false;
            if (debugFootsteps)
                Debug.Log("[AudioManager] Player footsteps STOP");
        }
    }

    // ── Neighbour footsteps ───────────────────────────────────────────────────

    public void StartFootsteps()
    {
        if (_neighbourFootstepsPlaying) return;
        _neighbourFootstepsInstance.start();
        _neighbourFootstepsPlaying = true;
    }

    public void StopFootsteps()
    {
        if (!_neighbourFootstepsPlaying) return;
        _neighbourFootstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _neighbourFootstepsPlaying = false;
    }

    public void UpdateNeighbourFootstepPosition(Vector3 worldPos)
    {
        _neighbourFootstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
    }

    // ── One-shots ────────────────────────────────────────────────────────────

    public void PlayNudge(float nudgeQuality = 0.5f)
    {
        EventInstance inst = RuntimeManager.CreateInstance(EVT_NUDGE);
        inst.setParameterByName("Nudge Quality", nudgeQuality);
        inst.start();
        inst.release();
    }

    public void PlayBirdChirp(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_BIRD_CHIRP, worldPos);

    public void PlayWateringCan(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_WATERING_CAN, worldPos);

    public void PlayWindchime(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_WINDCHIME, worldPos);

    public void PlayUIPop()
        => RuntimeManager.PlayOneShot(EVT_UI_POP);

    public void PlayPauseTransition()
        => RuntimeManager.PlayOneShot(EVT_PAUSE_TRANSITION);

    public void StopMusic()
        => _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

    public void SetWalkSurface(float value)
        => _neighbourFootstepsInstance.setParameterByName("Walk Surface", value);

    public void SetAmbienceVolume(float volume)
        => _ambienceInstance.setVolume(volume);
}