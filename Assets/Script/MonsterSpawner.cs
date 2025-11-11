using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject monsterPrefab;
    public int maxMonsters = 5;
    public float spawnInterval = 3f;
    public float spawnRadius = 5f;

    private float timer;
    private List<GameObject> activeMonsters = new List<GameObject>();

    void Update()
    {
        // Odstraò reference na mrtvé monstra (null po Destroy)
        activeMonsters.RemoveAll(m => m == null);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            if (activeMonsters.Count < maxMonsters)
            {
                SpawnMonster();
            }
        }
    }

    void SpawnMonster()
    {
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);

        // Pøedáme spawner monsteru, aby mohl hlásit smrt
        EnemyHealth enemyHealth = monster.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += () => OnMonsterDeath(monster);
        }

        activeMonsters.Add(monster);
    }

    void OnMonsterDeath(GameObject monster)
    {
        activeMonsters.Remove(monster);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
