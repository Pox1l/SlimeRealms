using UnityEngine;
using FMODUnity;

public class GlobalAudioSettings : MonoBehaviour
{
    void Awake()
    {
        // Najde všechny slidery pod tímto objektem, i ty v neaktivních panelech
        FMODVolumeControl[] allSliders = GetComponentsInChildren<FMODVolumeControl>(true);

        if (allSliders.Length == 0)
        {
            Debug.LogWarning("GlobalAudioSettings: Žádné slidery nenalezeny! Zkontroluj hierarchii.");
        }

        foreach (var ctrl in allSliders)
        {
            if (ctrl == null) continue;

            float savedVol = PlayerPrefs.GetFloat(ctrl.saveKey, 0.5f);
            FMOD.Studio.Bus bus = RuntimeManager.GetBus(ctrl.busPath);
            bus.setVolume(savedVol);

            Debug.Log($"[FMOD Awake] {ctrl.busPath} nastaven na {savedVol}");
        }
    }
}