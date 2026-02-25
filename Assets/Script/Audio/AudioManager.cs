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

    [Tooltip("Univerzální zvuk zásahu, když enemy nemá svùj vlastní")]
    public EventReference enemyHitSound;

    [Header("Default Attack Sounds")]
    [Tooltip("Univerzální švihnutí (pro Melee enemy)")]
    public EventReference defaultMeleeAttackSound;

    [Tooltip("Univerzální výstøel (pro Ranged enemy)")]
    public EventReference defaultRangedAttackSound;

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

    public void PlayPickupSound(EventReference specificSound)
    {
        if (!specificSound.IsNull)
        {
            RuntimeManager.PlayOneShot(specificSound);
        }
        else if (!defaultPickupSound.IsNull)
        {
            RuntimeManager.PlayOneShot(defaultPickupSound);
        }
    }

    // --- UPRAVENO PRO 3D ZVUK (MELEE) ---
    public void PlayMeleeAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull)
        {
            // Hrajeme custom zvuk na pozici nepøítele
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!defaultMeleeAttackSound.IsNull)
        {
            // Hrajeme defaultní zvuk na pozici nepøítele
            RuntimeManager.PlayOneShot(defaultMeleeAttackSound, worldPos);
        }
    }

    // --- UPRAVENO PRO 3D ZVUK (RANGED) ---
    public void PlayRangedAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull)
        {
            // Hrajeme custom výstøel na pozici zbranì/nepøítele
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!defaultRangedAttackSound.IsNull)
        {
            // Hrajeme default výstøel na pozici
            RuntimeManager.PlayOneShot(defaultRangedAttackSound, worldPos);
        }
    }

    public void PlayHitSound(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull)
        {
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!enemyHitSound.IsNull)
        {
            RuntimeManager.PlayOneShot(enemyHitSound, worldPos);
        }
        else
        {
            Debug.LogWarning("Chybí zvuk zásahu (enemyHitSound) v AudioManageru!");
        }
    }

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