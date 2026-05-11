using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// Attach this to any GameObject in the TitleScreen scene (e.g. the same
// GameObject that has TitleScreenUI on it).
// It starts the music on Awake and stops it cleanly when the scene unloads.

public class TitleAudioManager : MonoBehaviour
{
    const string EVT_MUSIC   = "event:/Music Loop";
    const string EVT_AMBIENCE = "event:/Level Ambience";

    [Header("Settings")]
    public bool playAmbience = false; // optional — enable if you want ambience too

    EventInstance _musicInstance;
    EventInstance _ambienceInstance;

    void Awake()
    {
        _musicInstance = RuntimeManager.CreateInstance(EVT_MUSIC);
        _musicInstance.start();

        if (playAmbience)
        {
            _ambienceInstance = RuntimeManager.CreateInstance(EVT_AMBIENCE);
            _ambienceInstance.start();
        }
    }

    void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();

        if (playAmbience)
        {
            _ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _ambienceInstance.release();
        }
    }
}