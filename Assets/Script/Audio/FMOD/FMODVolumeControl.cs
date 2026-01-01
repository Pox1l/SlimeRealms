using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class FMODVolumeControl : MonoBehaviour
{
    [Header("Cesta k Mixer Group (Bus)")]
    public string busPath = "bus:/";

    public string saveKey = "Master";

    private Slider slider;

    IEnumerator Start()
    {
        slider = GetComponent<Slider>();

        while (!RuntimeManager.HaveMasterBanksLoaded)
        {
            yield return null;
        }

        FMOD.Studio.Bus bus = RuntimeManager.GetBus(busPath);

        // --- NAÈÍTÁNÍ ---
        // Naèteme uloženou hodnotu. Pokud ještì neexistuje, použijeme 1 (plná hlasitost).
        float savedVol = PlayerPrefs.GetFloat(saveKey, 0.5f);

        // Nastavíme slider i FMOD bus na tuhle hodnotu
        slider.value = savedVol;
        bus.setVolume(savedVol);

        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        FMOD.Studio.Bus bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(volume);

        // --- UKLÁDÁNÍ ---
        // Kdykoliv pohneš sliderem, hodnota se hned uloží do poèítaèe
        PlayerPrefs.SetFloat(saveKey, volume);
        PlayerPrefs.Save();
    }
}