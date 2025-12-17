using UnityEngine;
using System.Collections;
using UnityEngine.AI; // Přidáno pro NavMeshAgent

[RequireComponent(typeof(Collider2D))]
public class SpawnerZone2D : MonoBehaviour
{
    [Header("Spawn nastavení")]
    public GameObject enemyPrefab;
    public int prewarmCount = 5;
    public int maxActive = 6;
    public float spawnInterval = 2f;
    public float spawnRadius = 5f;
    public float firstSpawnDelay = 0.5f;

    [Header("Kolize při spawnu")] // 👇 NOVÉ: Nastavení pro prevenci překrývání
    public float checkRadius = 1f; // Velikost prostoru, který enemy potřebuje
    public LayerMask obstacleLayer;  // Vrstvy, kterým se vyhýbáme (např. Enemy, Walls)

    private ObjectPool pool;
    private bool playerInside;
    private int activeCount;
    private Coroutine spawnLoop;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        pool = new ObjectPool(enemyPrefab, prewarmCount, transform);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        if (spawnLoop == null)
            spawnLoop = StartCoroutine(SpawnLoop());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DespawnAllEnemies());
        }
        else
        {
            DespawnAllEnemiesImmediate();
        }
    }


    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        while (playerInside)
        {
            if (activeCount < maxActive)
            {
                SpawnOneEnemy();
            }

            yield return wait;
        }
    }

    void SpawnOneEnemy()
    {
        // 👇 NOVÉ: Hledání bezpečné pozice (max 10 pokusů)
        Vector3 spawnPos = Vector3.zero;
        bool validPosFound = false;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            // Zkontroluje, zda je v bodě randomPoint v okruhu checkRadius něco z vrstvy obstacleLayer
            if (!Physics2D.OverlapCircle(randomPoint, checkRadius, obstacleLayer))
            {
                spawnPos = new Vector3(randomPoint.x, randomPoint.y, 0f);
                validPosFound = true;
                break; // Našli jsme místo, vyskočíme ze smyčky
            }
        }

        // Pokud jsme po 10 pokusech nenašli místo, spawn zrušíme a zkusíme to v příštím cyklu
        if (!validPosFound) return;


        // 👇 Zbytek je stejný, jen už používáme ověřenou 'spawnPos'
        var enemy = pool.Get();

        // 🔹 pro NavMeshAgenta je lepší použít Warp
        var agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos);
        }
        else
        {
            enemy.transform.position = spawnPos;
        }

        // 🔹 nastavíme enemymu domovskou pozici
        var ctrl = enemy.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            ctrl.SetHomePosition(spawnPos);
        }

        var ret = enemy.GetComponent<ReturnToPoolOnDeath>();
        if (ret != null)
        {
            ret.Init(pool, OnEnemyReturned);
        }

        activeCount++;
    }


    void OnEnemyReturned()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
    }

    IEnumerator DespawnAllEnemies()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                var ret = child.GetComponent<ReturnToPoolOnDeath>();
                if (ret != null)
                {
                    ret.ForceReturn();
                }

                yield return null;
            }
        }
    }

    void DespawnAllEnemiesImmediate()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                var ret = child.GetComponent<ReturnToPoolOnDeath>();
                if (ret != null)
                {
                    ret.ForceReturn();
                }
            }
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}