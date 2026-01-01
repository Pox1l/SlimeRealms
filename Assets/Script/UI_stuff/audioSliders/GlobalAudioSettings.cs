using System.Collections;
using UnityEngine;
using FMODUnity;

public class GlobalAudioSettings : MonoBehaviour
{
    IEnumerator Start()
    {
        // Poèkáme na banky
        while (!RuntimeManager.HaveMasterBanksLoaded)
        {
            yield return null;
        }

        // DÙLEŽITÉ: true v závorce zajistí, že najde i vypnuté (schované) slidery
        FMODVolumeControl[] allSliders = GetComponentsInChildren<FMODVolumeControl>(true);

        foreach (var ctrl in allSliders)
        {
            if (ctrl == null) continue;

            string path = ctrl.busPath;
            string key = ctrl.saveKey;

            // Naètení z PlayerPrefs
            float savedVol = PlayerPrefs.GetFloat(key, 1f);

            // Aplikace do FMODu
            FMOD.Studio.Bus bus = RuntimeManager.GetBus(path);
            bus.setVolume(savedVol);

            Debug.Log($"[Global Start] Nastaven {path} na {savedVol} pøes child hledání.");
        }
    }
}