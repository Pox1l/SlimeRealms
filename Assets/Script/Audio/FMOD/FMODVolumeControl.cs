using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity; // Dùležité pro propojení

public class FMODVolumeControl : MonoBehaviour
{
    [Header("Cesta k Mixer Group (Bus)")]
    [Tooltip("Sem napiš pøesnì: bus:/Master, bus:/Music nebo bus:/SFX")]
    public string busPath = "bus:/Master";

    private Slider slider;

    IEnumerator Start()
    {
        slider = GetComponent<Slider>();

        // KLÍÈOVÁ ÈÁST: Èekáme ve smyèce, dokud FMOD nenaète banky
        while (!RuntimeManager.HaveMasterBanksLoaded)
        {
            yield return null; // Poèkáme jeden snímek a zkusíme to znovu
        }

        // Teï už jsou banky naètené a bezpeènì najdeme Bus
        FMOD.Studio.Bus bus = RuntimeManager.GetBus(busPath);
        bus.getVolume(out float currentVol);
        slider.value = currentVol;

        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        FMOD.Studio.Bus bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(volume);
    }
}