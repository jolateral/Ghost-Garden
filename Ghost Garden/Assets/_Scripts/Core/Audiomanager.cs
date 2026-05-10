using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// Central audio manager. Attach to the GameManager GameObject.
// All other scripts call AudioManager.Instance.PlayXxx() rather than
// touching FMOD directly, so event paths are only defined in one place.

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ── FMOD event paths (match your strings bank exactly) ──────────────────
    const string EVT_BIRD_CHIRP       = "event:/Bird Chirp";
    const string EVT_FOOTSTEPS        = "event:/Footsteps";
    const string EVT_AMBIENCE         = "event:/Level Ambience";
    const string EVT_MUSIC            = "event:/Music Loop";
    const string EVT_NUDGE            = "event:/Nudge";
    const string EVT_PAUSE_TRANSITION = "event:/Pause Transition";
    const string EVT_UI_POP           = "event:/UI Pop";
    const string EVT_WATERING_CAN     = "event:/Watering Can + Shelf";
    const string EVT_WINDCHIME        = "event:/Windchime";

    // ── Persistent looping instances ─────────────────────────────────────────
    EventInstance _musicInstance;
    EventInstance _ambienceInstance;
    EventInstance _footstepsInstance;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Start looping music and ambience immediately
        _musicInstance   = RuntimeManager.CreateInstance(EVT_MUSIC);
        _ambienceInstance = RuntimeManager.CreateInstance(EVT_AMBIENCE);
        _musicInstance.start();
        _ambienceInstance.start();

        // Footsteps instance is started/stopped by NeighbourAI
        _footstepsInstance = RuntimeManager.CreateInstance(EVT_FOOTSTEPS);
    }

    void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();
        _ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _ambienceInstance.release();
        _footstepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _footstepsInstance.release();
    }

    // ── One-shot helpers ─────────────────────────────────────────────────────

    // Play a nudge sound. Pass nudgeQuality 0-1 to drive the FMOD parameter
    // (0 = wrong nudge / nothing happens, 1 = correct sequence nudge)
    public void PlayNudge(float nudgeQuality = 0.5f)
    {
        EventInstance inst = RuntimeManager.CreateInstance(EVT_NUDGE);
        inst.setParameterByName("Nudge Quality", nudgeQuality);
        inst.start();
        inst.release();
    }

    public void PlayBirdChirp(Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(EVT_BIRD_CHIRP, worldPos);
    }

    public void PlayWateringCan(Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(EVT_WATERING_CAN, worldPos);
    }

    public void PlayWindchime(Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(EVT_WINDCHIME, worldPos);
    }

    public void PlayUIPop()
    {
        RuntimeManager.PlayOneShot(EVT_UI_POP);
    }

    public void PlayPauseTransition()
    {
        RuntimeManager.PlayOneShot(EVT_PAUSE_TRANSITION);
    }

    // ── Footsteps (called by NeighbourAI) ────────────────────────────────────

    public void StartFootsteps()
    {
        _footstepsInstance.start();
    }

    public void StopFootsteps()
    {
        _footstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    // Set the Walk Surface parameter (e.g. 0 = grass, 1 = path, 2 = gravel)
    // Match these values to what you set up in FMOD Studio
    public void SetWalkSurface(float value)
    {
        _footstepsInstance.setParameterByName("Walk Surface", value);
    }

    // ── Music helpers ────────────────────────────────────────────────────────

    public void StopMusic()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void SetAmbienceVolume(float volume)
    {
        // volume 0-1
        _ambienceInstance.setVolume(volume);
    }
}