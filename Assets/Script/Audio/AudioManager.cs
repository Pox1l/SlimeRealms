using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Nastavení FMOD Eventù")]
    [Tooltip("Sem pøetáhni event s hudbou PRO MENU")]
    public EventReference menuMusicEvent;

    [Tooltip("Sem pøetáhni event s hlavní hudbou PRO HRU")]
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

    // --- PØIDÁNO PRO MANAGEMENT ZÓN A BOJE ---
    [HideInInspector] public bool isBossDead = false;
    private float currentBaseZone = 0f; // Pamatuje si, kde hráè je, když zrovna nebojuje
    private int enemiesInCombat = 0; // Kolik nepøátel hráèe aktuálnì vidí

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (!ambientEvent.IsNull)
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
            ambientInstance.start();
        }

        PlayCorrectMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayCorrectMusicForScene(scene.buildIndex);
    }

    private void PlayCorrectMusicForScene(int sceneIndex)
    {
        EventReference correctEvent = (sceneIndex == 0) ? menuMusicEvent : musicEvent;

        if (correctEvent.IsNull) return;

        if (musicInstance.isValid())
        {
            musicInstance.getDescription(out EventDescription currentDesc);
            currentDesc.getID(out FMOD.GUID currentID);

            if (currentID == correctEvent.Guid) return;

            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        musicInstance = RuntimeManager.CreateInstance(correctEvent);
        musicInstance.start();
    }

    // --- UPRAVENO: Ukládání aktuální zóny ---
    public void SetZone(float zoneID)
    {
        currentBaseZone = zoneID; // Uložíme si zónu

        // Pokud jsme zrovna v combatu, nepøepisujeme hudbu zpìt na chill
        if (enemiesInCombat > 0 && zoneID < 2f) return;

        if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", zoneID);
        if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", zoneID);
    }

    // --- PØIDÁNO: Dynamický combat systém ---
    public void SetCombatState(bool inCombat)
    {
        if (inCombat) enemiesInCombat++;
        else enemiesInCombat--;

        enemiesInCombat = Mathf.Max(0, enemiesInCombat); // Nesmí jít do mínusu

        if (enemiesInCombat > 0)
        {
            // Pøepne na Battle (Zone 2)
            if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", 2f);
            if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", 2f);
        }
        else
        {
            // Vrátí se do normální zóny, kde hráè zrovna stojí
            if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", currentBaseZone);
            if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", currentBaseZone);
        }
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

    public void PlayMeleeAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull)
        {
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!defaultMeleeAttackSound.IsNull)
        {
            RuntimeManager.PlayOneShot(defaultMeleeAttackSound, worldPos);
        }
    }

    public void PlayRangedAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull)
        {
            RuntimeManager.PlayOneShot(specificSound, worldPos);
        }
        else if (!defaultRangedAttackSound.IsNull)
        {
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