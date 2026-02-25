using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Nastavení FMOD Eventù")]
    [Tooltip("Sem pøetáhni event s hudbou")]
    public EventReference musicEvent;

    [Tooltip("Sem pøetáhni event s ambientem")]
    public EventReference ambientEvent;

    [Header("Default Sounds")]
    [Tooltip("Zvuk, který se pøehraje, když item nemá svùj vlastní")]
    public EventReference defaultPickupSound;

    private EventInstance musicInstance;
    private EventInstance ambientInstance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!musicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();
        }

        if (!ambientEvent.IsNull)
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
            ambientInstance.start();
        }
    }

    public void SetZone(float zoneID)
    {
        if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", zoneID);
        if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", zoneID);
    }

    // --- TOTO JE TA NOVÁ FUNKCE ---
    // Voláš ji z itemu a jen pošleš zvuk z ItemSO. Manager rozhodne zbytek.
    public void PlayPickupSound(EventReference specificSound)
    {
        // 1. Má item svùj vlastní zvuk? (není Null)
        if (!specificSound.IsNull)
        {
            RuntimeManager.PlayOneShot(specificSound);
        }
        // 2. Nemá? Tak pøehrajeme defaultní zvuk z Manageru
        else if (!defaultPickupSound.IsNull)
        {
            RuntimeManager.PlayOneShot(defaultPickupSound);
        }
        // 3. Pokud není ani defaultní, nestane se nic (ticho)
    }
    // -----------------------------

    public void PlayOneShot(EventReference sound)
    {
        if (!sound.IsNull)
        {
            RuntimeManager.PlayOneShot(sound);
        }
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }

        if (ambientInstance.isValid())
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            ambientInstance.release();
        }
    }
}