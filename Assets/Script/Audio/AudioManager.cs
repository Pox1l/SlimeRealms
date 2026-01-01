using UnityEngine;
using System.Collections; // <--- TOTO MUSÍŠ PØIDAT
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("FMOD Events")]
    public EventReference musicEvent;
    public EventReference uiClickSound;

    private EventInstance musicInstance;

    public const string MASTER_KEY = "MasterVolume";
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ZMÌNA ZDE: void -> IEnumerator
    private IEnumerator Start()
    {
        // 1. Èekáme, dokud FMOD nenaète banky (Master a Strings)
        // Bez tohohle to spadne, protože GetBus nenajde cestu
        while (!RuntimeManager.HaveMasterBanksLoaded)
        {
            yield return null;
        }

        // 2. Teï už je bezpeèné naèítat a nastavovat hlasitost
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(MUSIC_KEY, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, 1f));

        // 3. Spustíme hudbu
        PlayMusic();
    }

    public void PlayMusic()
    {
        PLAYBACK_STATE playbackState;
        musicInstance.getPlaybackState(out playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();
        }
    }

    public void PlayOneShot(EventReference sound)
    {
        if (!sound.IsNull)
        {
            RuntimeManager.PlayOneShot(sound, transform.position);
        }
    }

    public void SetMasterVolume(float value)
    {
        // Zde mùžeš pøidat pojistku, kdyby to nìkdo volal moc brzy zvenèí
        if (RuntimeManager.HaveMasterBanksLoaded)
        {
            RuntimeManager.GetBus("bus:/").setVolume(value);
        }
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetMusicVolume(float value)
    {
        if (RuntimeManager.HaveMasterBanksLoaded)
        {
            RuntimeManager.GetBus("bus:/Music").setVolume(value);
        }
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        if (RuntimeManager.HaveMasterBanksLoaded)
        {
            RuntimeManager.GetBus("bus:/SFX").setVolume(value);
        }
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }
}