using UnityEngine;
using UnityEngine.AI; // Nutné pro NavMeshAgent

public class BossMinionSpawner : MonoBehaviour
{
    [Header("Co spawnovat")]
    public GameObject enemyPrefab;
    public int spawnAmount = 1; // Kolik nepøátel se má z tohoto spawneru objevit naráz

    [Header("Nastavení Pozice")]
    public float spawnRadius = 2f; // Jak moc se mohou rozptýlit
    public float checkRadius = 0.5f; // Velikost nepøítele (pro kontrolu kolize)
    public LayerMask obstacleLayer;  // Zdi a pøekážky (nastav v inspektoru!)

    [Header("Pool Settings")]
    public int prewarmCount = 2; // Kolik si jich pøipravit do pamìti

    private ObjectPool pool;
    private bool hasSpawned = false; // Pojistka, aby se to stalo jen jednou za fight

    void Awake()
    {
        // Inicializace Poolu (stejné jako ve tvém kódu)
        if (enemyPrefab != null)
        {
            pool = new ObjectPool(enemyPrefab, prewarmCount, transform);
        }
    }

    void OnEnable()
    {
        // Pøihlásíme se k události Bosse (fáze 2)
        BossController.OnPhase2Start += SpawnWave;
    }

    void OnDisable()
    {
        BossController.OnPhase2Start -= SpawnWave;
    }

    // Tato metoda se zavolá automaticky, když Boss køikne "Fáze 2!"
    void SpawnWave()
    {
        if (hasSpawned) return; // Už jsme spawnovali, konèíme
        hasSpawned = true;

        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnOneEnemy();
        }
    }

    void SpawnOneEnemy()
    {
        if (pool == null) return;

        // 1. Hledání bezpeèné pozice (tvá logika)
        Vector3 spawnPos = transform.position; // Defaultnì støed spawneru
        bool validPosFound = false;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            // Pokud v místì NENÍ zeï/pøekážka
            if (!Physics2D.OverlapCircle(randomPoint, checkRadius, obstacleLayer))
            {
                spawnPos = new Vector3(randomPoint.x, randomPoint.y, 0f);
                validPosFound = true;
                break;
            }
        }

       
        if (!validPosFound)
        {
            Debug.LogWarning($"Spawner {name}: Nenašel bezpeèné místo, spawnuje na støedu.");
        }

        // 2. Vytažení z Poolu
        GameObject enemy = pool.Get();

        // 3. Nastavení pozice (NavMesh Warp vs Transform)
        var agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos); // Warp je pro agenty nutný, jinak ignorují transform.position
        }
        else
        {
            enemy.transform.position = spawnPos;
        }

        // 4. Nastavení AI (Domovská pozice)
        var ctrl = enemy.GetComponent<EnemyController>(); // Pokud máš tento skript
        if (ctrl != null)
        {
            ctrl.SetHomePosition(spawnPos);
        }

        // 5. Nastavení návratu do poolu pøi smrti
        var ret = enemy.GetComponent<ReturnToPoolOnDeath>();
        if (ret != null)
        {
            // Callback 'null', protože tady nepotøebujeme poèítat activeCount jako ve vlnách
            ret.Init(pool, null);
        }
    }

    // Volitelné: Pro vizualizaci v editoru
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}