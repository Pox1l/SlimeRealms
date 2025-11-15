using UnityEngine;
using System.Collections;

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

        // 👇 přidat tuhle podmínku
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DespawnAllEnemies());
        }
        else
        {
            // volitelně: můžeš to udělat sync bez coroutine
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
        var enemy = pool.Get();

        // vypočítáme spawn pozici
        Vector2 spawnPos2D = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(spawnPos2D.x, spawnPos2D.y, 0f);

        // 🔹 pro NavMeshAgenta je lepší použít Warp
        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos); // bezpečný "teleport" na NavMesh
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
                    ret.ForceReturn(); // to už zavolá OnEnemyReturned
                }

                // rozprostření přes více framů
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
