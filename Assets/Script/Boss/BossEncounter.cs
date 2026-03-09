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
    public BossEntrance entranceScript; // Toto se teď bude hledat samo

    private ObjectPool pool;
    private GameObject activeBoss;
    private Coroutine spawnCoroutine;
    private Coroutine barrierCoroutine;
    private bool playerInside = false;

    // 🔥 POMOCNÁ PROMĚNNÁ: Pamatuje si, jestli boss už nahodil aggro, abychom to neodečítali víckrát
    private bool hasBossAggro = false;

    void Awake()
    {
        pool = new ObjectPool(bossPrefab, 1, transform);

        if (barrierObject != null) barrierObject.SetActive(false);

        // 1. Automatické nalezení zoomeru
        if (cameraZoomer == null)
        {
            cameraZoomer = FindAnyObjectByType<PixelCameraZoomer>();
        }

        // 2. NOVÉ: Automatické hledání BossEntrance podle TAGU
        if (entranceScript == null)
        {
            // Hledáme objekt s tagem "BossEntrance"
            GameObject entObj = GameObject.FindGameObjectWithTag("BossEntrance");

            if (entObj != null)
            {
                entranceScript = entObj.GetComponent<BossEntrance>();
            }
            else
            {
                // Záloha: Pokud tag nenajde, zkusí najít jakýkoliv objekt s tímto skriptem
                entranceScript = FindAnyObjectByType<BossEntrance>();
                if (entranceScript == null)
                    Debug.LogWarning("BossEncounter: Nenalezen 'BossEntrance'! Ujisti se, že máš objekt s tagem 'BossEntrance'.");
            }
        }
    }

    public void PrepareBoss()
    {
        bossDefeated = false;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.isBossDead = false;
        }

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

        if (barrierObject != null && !bossDefeated)
        {
            if (barrierCoroutine != null) StopCoroutine(barrierCoroutine);
            barrierCoroutine = StartCoroutine(ActivateBarrierWithDelay());
        }

        if (cameraZoomer != null) cameraZoomer.ZoomToCombat();

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

        if (barrierCoroutine != null)
        {
            StopCoroutine(barrierCoroutine);
            barrierCoroutine = null;
        }

        if (barrierObject != null) barrierObject.SetActive(false);

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

    // 🔥 VOLÁ SE PŘI ZABITÍ BOSSE
    public void SetBossDefeated()
    {
        bossDefeated = true;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.isBossDead = true;

            // 1. Odečteme aggro bosse, protože zemřel
            if (hasBossAggro)
            {
                AudioManager.instance.RemoveAggro();
                hasBossAggro = false;
            }

            // 2. Vynutíme Victory znělku (Zone 4) nezávisle na tom, kolik minionů žije
            AudioManager.instance.SetZone(4f);
        }

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine);
        if (barrierObject != null) barrierObject.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.EndBossFight();

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();
    }

    IEnumerator ActivateBarrierWithDelay()
    {
        yield return new WaitForSeconds(barrierDelay);

        if (playerInside && !bossDefeated && barrierObject != null)
        {
            barrierObject.SetActive(true);

            // 🔥 Když se zavře bariéra, bereme to jako začátek boje (Aggro)
            if (AudioManager.instance != null && !hasBossAggro)
            {
                AudioManager.instance.AddAggro();
                hasBossAggro = true;
            }
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

            // 🔥 Pokud boss zmizí (hráč utekl z místnosti), zrušíme hudbu boje
            if (AudioManager.instance != null && hasBossAggro)
            {
                AudioManager.instance.RemoveAggro();
                hasBossAggro = false;
            }
        }
    }
}