using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Auto-Filled Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    private void Awake()
    {
        FindSlidersByName();
    }

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogWarning("Chybí AudioManager!");
            return;
        }

        if (!CheckSlidersExist()) return;

        // ZMÌNA: Naèítáme uloženou hodnotu slideru (0 až 1) z PlayerPrefs
        // Používáme klíèe z AudioManageru, aby to sedìlo
        masterSlider.value = PlayerPrefs.GetFloat(AudioManager.MASTER_KEY, 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 0.5f);
        uiSlider.value = PlayerPrefs.GetFloat(AudioManager.UI_KEY, 0.5f);

        // Propojení funkcí
        masterSlider.onValueChanged.AddListener(AudioManager.instance.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
        uiSlider.onValueChanged.AddListener(AudioManager.instance.SetUIVolume);
    }

    private void FindSlidersByName()
    {
        Slider[] allSliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider s in allSliders)
        {
            switch (s.gameObject.name)
            {
                case "MasterVolumeSL": masterSlider = s; break;
                case "MusicVolumeSL": musicSlider = s; break;
                case "SFXVolumeSL": sfxSlider = s; break;
                case "UIVolumeSL": uiSlider = s; break;
            }
        }
    }

    private bool CheckSlidersExist()
    {
        if (masterSlider == null || musicSlider == null || sfxSlider == null || uiSlider == null)
        {
            Debug.LogError("CHYBA: AudioSettingsUI nenašel nìkteré slidery! Zkontroluj názvy.");
            return false;
        }
        return true;
    }
}