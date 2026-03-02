using UnityEngine;
using UnityEngine.SceneManagement; // PØIDÁNO: Pro detekci aktuální scény
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Nastavení FMOD Eventù")]
    [Tooltip("Sem pøetáhni event s hudbou PRO MENU")]
    public EventReference menuMusicEvent; // PØIDÁNO: Oddìlená hudba pro menu

    [Tooltip("Sem pøetáhni event s hlavní hudbou PRO HRU")]
    public EventReference musicEvent;

    [Tooltip("Sem pøetáhni event s ambientem")]
    public EventReference ambientEvent;

    [Header("Default Sounds")]
    [Tooltip("Zvuk, kterı se pøehraje, kdy item nemá svùj vlastní")]
    public EventReference defaultPickupSound;

    [Tooltip("Univerzální zvuk zásahu, kdy enemy nemá svùj vlastní")]
    public EventReference enemyHitSound;

    [Header("Default Attack Sounds")]
    [Tooltip("Univerzální švihnutí (pro Melee enemy)")]
    public EventReference defaultMeleeAttackSound;

    [Tooltip("Univerzální vıstøel (pro Ranged enemy)")]
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

    // PØIDÁNO: Pøihlášení k eventu naètení scény
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // PØIDÁNO: Odhlášení z eventu (prevence errorù)
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Ambient se spustí normálnì
        if (!ambientEvent.IsNull)
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
            ambientInstance.start();
        }

        // PØIDÁNO: První spuštìní hudby podle aktuální scény
        PlayCorrectMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    // PØIDÁNO: Automatické pøepnutí pøi zmìnì scény
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayCorrectMusicForScene(scene.buildIndex);
    }

    // PØIDÁNO: Logika pro správnı vıbìr hudby
    private void PlayCorrectMusicForScene(int sceneIndex)
    {
        EventReference correctEvent = (sceneIndex == 0) ? menuMusicEvent : musicEvent;

        if (correctEvent.IsNull) return;

        // Pokud u hraje správná hudba, nic nemìò
        if (musicInstance.isValid())
        {
            musicInstance.getDescription(out EventDescription currentDesc);
            currentDesc.getID(out FMOD.GUID currentID);

            if (currentID == correctEvent.Guid) return;

            // Zastav pøedchozí hudbu s fadem
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        // Spus novou hudbu
        musicInstance = RuntimeManager.CreateInstance(correctEvent);
        musicInstance.start();
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
            // Hrajeme custom vıstøel na pozici zbranì/nepøítele
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!defaultRangedAttackSound.IsNull)
        {
            // Hrajeme default vıstøel na pozici
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