using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("Nastavení Bosse")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float startDelay = 0.5f;
    public bool bossDefeated = false; // Pokud true, boss už se neobjeví

    [Header("Propojení")]
    public PixelCameraZoomer cameraZoomer;

    // Interní proměnné pro Pool
    private ObjectPool pool;
    private GameObject activeBoss;
    private Coroutine spawnCoroutine;
    private bool playerInside = false;

    void Awake()
    {
        // Inicializace Poolu - stačí nám 1 boss (prewarm 1)
        // Předpokládám, že máš třídu ObjectPool definovanou stejně jako v předchozím příkladu
        pool = new ObjectPool(bossPrefab, 1, transform);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // 1. Zoom kamery
        if (cameraZoomer != null) cameraZoomer.ZoomToCombat();

        // 2. Spawn sekvence (pokud boss žije a není zrovna aktivní)
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

        // 3. Zoom zpět
        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();

        // 4. Reset boje - Boss zmizí (vrátí se do poolu), pokud jsi ho nezabil
        if (activeBoss != null)
        {
            DespawnBoss();
        }
    }

    IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(startDelay);

        // Pojistka: Hráč mohl během delaye odejít
        if (playerInside && activeBoss == null && !bossDefeated)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        activeBoss = pool.Get();

        // 1. Přesuneme bosse na místo
        Vector3 finalPos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        var agent = activeBoss.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null) agent.Warp(finalPos);
        else activeBoss.transform.position = finalPos;

        // --- 👇 TOTO MUSÍŠ PŘIDAT 👇 ---
        // Řekneme controlleru: "Zapomeň, kde ses narodil. Tady v aréně je tvůj nový domov."
        var controller = activeBoss.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.SetHomePosition(finalPos);
        }
        // -------------------------------

        // Zbytek tvého kódu pro návrat do poolu...
        var ret = activeBoss.GetComponent<ReturnToPoolBoss>();
        if (ret != null)
        {
            ret.Init(pool, OnBossReturned);
        }

        Debug.Log("👹 Boss Spawned z Poolu a má nastavený domov!");
    }

    void DespawnBoss()
    {
        if (activeBoss != null && activeBoss.activeSelf)
        {
            // 🔥 OPRAVA: Zase hledáme ReturnToPoolBoss
            var ret = activeBoss.GetComponent<ReturnToPoolBoss>();
            if (ret != null)
            {
                ret.ForceReturn();
            }
            else
            {
                activeBoss.SetActive(false);
            }
        }
    }

    // Callback volaný z ReturnToPoolOnDeath
    void OnBossReturned()
    {
        activeBoss = null;
    }

    // 🔥 Tuto metodu zavolej ze skriptu Bosse (z jeho metody Die), když opravdu umře
    public void SetBossDefeated()
    {
        bossDefeated = true;

        // Pokud chceš, aby po smrti rovnou zmizel (nebo nechal mrtvolu a zmizel až odejdeš),
        // upravuje se to v ReturnToPoolOnDeath na bossovi.

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();
        Debug.Log("🏆 Boss poražen navždy!");
    }
}