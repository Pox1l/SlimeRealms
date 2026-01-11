using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("Nastavení")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float startDelay = 0.5f;
    public string bossName = "Evil Boss";

    // ODSTRANĚNO: public int bossMaxHP = 500; -> Už to čteme přímo z bosse

    public bool bossDefeated = true;

    [Header("Propojení")]
    public PixelCameraZoomer cameraZoomer;
    public BossEntrance entranceScript;

    private ObjectPool pool;
    private GameObject activeBoss;
    private Coroutine spawnCoroutine;
    private bool playerInside = false;

    void Awake()
    {
        pool = new ObjectPool(bossPrefab, 1, transform);
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

        if (cameraZoomer != null) cameraZoomer.ZoomToCombat();

        // 🔥 OZNÁMENÍ MANAGERU: Začátek boje
        if (!bossDefeated && UIManager.Instance != null)
        {
            // Změna: Zjistíme HP přímo z prefabu bosse, místo natvrdo zadaného čísla
            int realMaxHP = 100; // Fallback hodnota
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

        // 🔥 OZNÁMENÍ MANAGERU: Konec boje (útěk)
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

        // 🔥 OZNÁMENÍ MANAGERU: Konec boje (smrt bosse)
        if (UIManager.Instance != null)
            UIManager.Instance.EndBossFight();

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();
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

        // 🔥 OPRAVA: Už nepřepisujeme MaxHealth, jen resetujeme CurrentHealth na MaxHealth
        if (activeBoss.TryGetComponent(out BossHealth hpScript))
        {
            // hpScript.maxHealth nechej tak, jak je nastavené v prefabu!
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