using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("Nastavení")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float startDelay = 0.5f;
    public string bossName = "Evil Boss";
    public bool bossDefeated = true;

    [Header("Bariéra")]
    public GameObject barrierObject; 
    public float barrierDelay = 1.0f; 

    [Header("Propojení")]
    public PixelCameraZoomer cameraZoomer;
    public BossEntrance entranceScript;

    private ObjectPool pool;
    private GameObject activeBoss;
    private Coroutine spawnCoroutine;
    private Coroutine barrierCoroutine; 
    private bool playerInside = false;

    void Awake()
    {
        pool = new ObjectPool(bossPrefab, 1, transform);

        if (barrierObject != null) barrierObject.SetActive(false);
    }

    public void PrepareBoss()
    {
        bossDefeated = false;
        if (playerInside && activeBoss == null)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnSequence());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        // 🔥 UPRAVENO: Bariéru nezapínáme hned, ale spustíme odpočet
        if (barrierObject != null && !bossDefeated)
        {
            if (barrierCoroutine != null) StopCoroutine(barrierCoroutine);
            barrierCoroutine = StartCoroutine(ActivateBarrierWithDelay());
        }

        if (cameraZoomer != null) cameraZoomer.ZoomToCombat();

        // OZNÁMENÍ MANAGERU: Začátek boje
        if (!bossDefeated && UIManager.Instance != null)
        {
            int realMaxHP = 100;
            if (bossPrefab != null && bossPrefab.TryGetComponent(out BossHealth hpScript))
            {
                realMaxHP = hpScript.maxHealth;
            }
            UIManager.Instance.StartBossFight(bossName, realMaxHP);
        }

        if (!bossDefeated && activeBoss == null)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnSequence());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        // 🔥 DŮLEŽITÉ: Pokud hráč odejde, okamžitě zrušíme čekání na bariéru
        if (barrierCoroutine != null)
        {
            StopCoroutine(barrierCoroutine);
            barrierCoroutine = null;
        }

        // A vypneme bariéru, pokud už byla aktivní
        if (barrierObject != null) barrierObject.SetActive(false);

        // OZNÁMENÍ MANAGERU: Konec boje (útěk)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.EndBossFight();
        }

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();

        if (activeBoss != null)
        {
            DespawnBoss();
        }
    }

    public void SetBossDefeated()
    {
        bossDefeated = true;

        // Zrušíme případný odpočet a vypneme bariéru
        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine);
        if (barrierObject != null) barrierObject.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.EndBossFight();

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();
    }

    // 🔥 NOVÁ COROUTINA: Čeká 1 sekundu a pak zkontroluje, jestli je hráč stále uvnitř
    IEnumerator ActivateBarrierWithDelay()
    {
        yield return new WaitForSeconds(barrierDelay);

        // Po čekání znovu ověříme podmínky:
        // 1. Hráč musí být stále uvnitř (playerInside == true)
        // 2. Boss nesmí být mrtvý
        if (playerInside && !bossDefeated && barrierObject != null)
        {
            barrierObject.SetActive(true);
        }
    }

    IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(startDelay);
        if (playerInside && activeBoss == null && !bossDefeated) SpawnBoss();
    }

    void SpawnBoss()
    {
        activeBoss = pool.Get();
        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        if (activeBoss.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) agent.Warp(pos);
        else activeBoss.transform.position = pos;

        if (activeBoss.TryGetComponent(out EnemyController ctrl)) ctrl.SetHomePosition(pos);
        if (activeBoss.TryGetComponent(out ReturnToPoolBoss ret)) ret.Init(pool, () => activeBoss = null);

        if (activeBoss.TryGetComponent(out BossHealth hpScript))
        {
            hpScript.currentHealth = hpScript.maxHealth;
        }
    }

    void DespawnBoss()
    {
        if (activeBoss != null && activeBoss.activeSelf)
        {
            if (activeBoss.TryGetComponent(out ReturnToPoolBoss ret)) ret.ForceReturn();
            else activeBoss.SetActive(false);
        }
    }
}