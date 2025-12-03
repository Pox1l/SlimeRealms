using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    // ZMÌNA: Musí být PUBLIC, aby je vidìl i UI skript
    public const string MASTER_KEY = "MasterVolume";
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";
    public const string UI_KEY = "UIVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadVolume(MASTER_KEY);
        LoadVolume(MUSIC_KEY);
        LoadVolume(SFX_KEY);
        LoadVolume(UI_KEY);
    }

    public void SetMasterVolume(float sliderValue) => SetMixerVolume(MASTER_KEY, sliderValue);
    public void SetMusicVolume(float sliderValue) => SetMixerVolume(MUSIC_KEY, sliderValue);
    public void SetSFXVolume(float sliderValue) => SetMixerVolume(SFX_KEY, sliderValue);
    public void SetUIVolume(float sliderValue) => SetMixerVolume(UI_KEY, sliderValue);

    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        PlayerPrefs.SetFloat(parameterName, sliderValue);
        PlayerPrefs.Save();

        if (sliderValue <= 0.001f)
        {
            audioMixer.SetFloat(parameterName, -80f);
            return;
        }

        float dbValue = Mathf.Log10(sliderValue) * 30;
        audioMixer.SetFloat(parameterName, dbValue);
    }

    private void LoadVolume(string paramName)
    {
        float savedValue = PlayerPrefs.GetFloat(paramName, 0.5f);
        SetMixerVolume(paramName, savedValue);
    }

    public void PlaySFX(AudioClip clip) { if (clip != null) sfxSource.PlayOneShot(clip); }
    public void PlayUISound(AudioClip clip) { if (clip != null) uiSource.PlayOneShot(clip); }
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
}