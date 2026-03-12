using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Nastavení FMOD Eventů")]
    public EventReference menuMusicEvent;
    public EventReference musicEvent;
    public EventReference ambientEvent;

    [Header("Default Sounds")]
    public EventReference defaultPickupSound;
    public EventReference enemyHitSound;
    public EventReference defaultPlayerHitSound; // 🔥 PŘIDÁNO

    [Tooltip("Univerzální zvuk všimnutí (Aggro), když enemy nemá svůj vlastní")]
    public EventReference defaultAggroSound;

    [Header("Default Attack Sounds")]
    public EventReference defaultMeleeAttackSound;
    public EventReference defaultRangedAttackSound;

    [Header("Combat Music Settings")]
    [Tooltip("Za jak dlouho (vteřiny) se hudba uklidní po skončení boje")]
    public float combatDropDelay = 4.0f;

    [HideInInspector] public bool isBossDead = false;
    private float currentBaseZone = 0f;
    private int enemiesInCombat = 0;

    private EventInstance musicInstance;
    private EventInstance ambientInstance;
    private Coroutine combatDropCoroutine;

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

    public void SetZone(float zoneID)
    {
        currentBaseZone = zoneID;

        if (enemiesInCombat > 0 && zoneID < 2f) return;

        if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", zoneID);
        if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", zoneID);
    }

    public void AddAggro()
    {
        enemiesInCombat++;

        if (combatDropCoroutine != null)
        {
            StopCoroutine(combatDropCoroutine);
            combatDropCoroutine = null;
        }

        UpdateCombatMusic();
    }

    public void RemoveAggro()
    {
        enemiesInCombat--;
        enemiesInCombat = Mathf.Max(0, enemiesInCombat);

        if (enemiesInCombat == 0)
        {
            if (combatDropCoroutine != null) StopCoroutine(combatDropCoroutine);
            combatDropCoroutine = StartCoroutine(DropCombatCooldown());
        }
    }

    private IEnumerator DropCombatCooldown()
    {
        yield return new WaitForSeconds(combatDropDelay);
        UpdateCombatMusic();
    }

    private void UpdateCombatMusic()
    {
        if (enemiesInCombat > 0)
        {
            if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", 2f);
            if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", 2f);
        }
        else
        {
            if (musicInstance.isValid()) musicInstance.setParameterByName("Zone", currentBaseZone);
            if (ambientInstance.isValid()) ambientInstance.setParameterByName("Zone", currentBaseZone);
        }
    }

    public void PlayAggroSound(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound, worldPos);
        else if (!defaultAggroSound.IsNull) RuntimeManager.PlayOneShot(defaultAggroSound, worldPos);
    }

    public void PlayPickupSound(EventReference specificSound)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound);
        else if (!defaultPickupSound.IsNull) RuntimeManager.PlayOneShot(defaultPickupSound);
    }

    public void PlayMeleeAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound, worldPos);
        else if (!defaultMeleeAttackSound.IsNull) RuntimeManager.PlayOneShot(defaultMeleeAttackSound, worldPos);
    }

    public void PlayRangedAttack(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound, worldPos);
        else if (!defaultRangedAttackSound.IsNull) RuntimeManager.PlayOneShot(defaultRangedAttackSound, worldPos);
    }

    public void PlayHitSound(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound, worldPos);
        else if (!enemyHitSound.IsNull) RuntimeManager.PlayOneShot(enemyHitSound, worldPos);
    }

    // 🔥 PŘIDÁNO: Zvuk zranění hráče
    public void PlayPlayerHitSound(EventReference specificSound, Vector3 worldPos)
    {
        if (!specificSound.IsNull) RuntimeManager.PlayOneShot(specificSound, worldPos);
        else if (!defaultPlayerHitSound.IsNull) RuntimeManager.PlayOneShot(defaultPlayerHitSound, worldPos);
    }

    public void PlayOneShot(EventReference sound)
    {
        if (!sound.IsNull) RuntimeManager.PlayOneShot(sound);
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