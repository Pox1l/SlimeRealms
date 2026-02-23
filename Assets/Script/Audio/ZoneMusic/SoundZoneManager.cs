using UnityEngine;
using FMODUnity;

public class SoundZoneManager : MonoBehaviour
{
    public static SoundZoneManager instance { get; private set; }

    [Header("Spustí se hned pøi naètení scény")]
    public EventReference sceneMusic;
    public EventReference sceneAmbient;

    [Header("Jednorázové zvuky")]
    public EventReference playerDeathEvent;

    private FMOD.Studio.EventInstance musicInstance;
    private FMOD.Studio.EventInstance ambientInstance;

    // ID parametru pro lepší výkon
    private FMOD.Studio.PARAMETER_ID zoneStateParamId;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    private void Start()
    {
        // 1. Spuštìní hlavní hudby
        musicInstance = RuntimeManager.CreateInstance(sceneMusic);
        musicInstance.start();

        // Získání ID parametru pro zóny/fighty (pojmenuj parametr ve FMODu "ZoneState")
        FMOD.Studio.EventDescription musicDesc;
        musicInstance.getDescription(out musicDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
        musicDesc.getParameterDescriptionByName("ZoneState", out paramDesc);
        zoneStateParamId = paramDesc.id;

        // 2. Spuštìní ambientu
        ambientInstance = RuntimeManager.CreateInstance(sceneAmbient);
        ambientInstance.start();
    }

    // Zmìna hudby podle zóny (FMOD parametr: 0 = klid, 1 = bitva, 2 = boss atd.)
    public void SetZoneState(float stateValue)
    {
        musicInstance.setParameterByID(zoneStateParamId, stateValue);
    }

    public void PlayPlayerDeath()
    {
        RuntimeManager.PlayOneShot(playerDeathEvent);
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDestroy()
    {
        musicInstance.release();
        ambientInstance.release();
    }
}