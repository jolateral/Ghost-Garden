using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ── Event paths from Master_strings.bank ─────────────────────────────────
    const string EVT_BIRD_CHIRP       = "event:/Bird Chirp";
    const string EVT_FOLIAGE_BRUSH    = "event:/Foliage Brush";  // new — trees/flowers
    const string EVT_FOOTSTEPS        = "event:/Footsteps";
    const string EVT_AMBIENCE         = "event:/Level Ambience";
    const string EVT_MUSIC            = "event:/Music Loop";
    const string EVT_NUDGE            = "event:/Nudge";
    const string EVT_PAUSE_TRANSITION = "event:/Pause Transition";
    const string EVT_THUD             = "event:/Thud";            // new — impact sound
    const string EVT_UI_POP           = "event:/UI Pop";
    const string EVT_WATERING_CAN     = "event:/Watering Can + Shelf";
    const string EVT_WINDCHIME        = "event:/Windchime";

    // ── Looping instances ─────────────────────────────────────────────────────
    EventInstance _musicInstance;
    EventInstance _ambienceInstance;
    EventInstance _playerFootstepsInstance;
    EventInstance _neighbourFootstepsInstance;

    bool _playerFootstepsPlaying;
    bool _neighbourFootstepsPlaying;

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

        _playerFootstepsInstance    = RuntimeManager.CreateInstance(EVT_FOOTSTEPS);
        _neighbourFootstepsInstance = RuntimeManager.CreateInstance(EVT_FOOTSTEPS);

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

    // ── Player footsteps ──────────────────────────────────────────────────────

    public void PlayPlayerFootstep(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_FOOTSTEPS, worldPos);

    public void SetPlayerFootstepsActive(bool isMoving, Vector3 worldPos)
    {
        if (!isMoving && _playerFootstepsPlaying)
        {
            _playerFootstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _playerFootstepsPlaying = false;
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
        => _neighbourFootstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));

    public void SetWalkSurface(float value)
        => _neighbourFootstepsInstance.setParameterByName("Walk Surface", value);

    // ── One-shots ─────────────────────────────────────────────────────────────

    public void PlayNudge(float nudgeQuality = 0.5f)
    {
        EventInstance inst = RuntimeManager.CreateInstance(EVT_NUDGE);
        inst.setParameterByName("Nudge Quality", nudgeQuality);
        inst.start();
        inst.release();
    }

    // Played when the player nudges a foliage object (tree, flower, bush)
    public void PlayFoliageBrush(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_FOLIAGE_BRUSH, worldPos);

    // Played on heavy impacts e.g. watering can hitting the ground
    public void PlayThud(Vector3 worldPos)
        => RuntimeManager.PlayOneShot(EVT_THUD, worldPos);

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

    public void SetAmbienceVolume(float volume)
        => _ambienceInstance.setVolume(volume);
}