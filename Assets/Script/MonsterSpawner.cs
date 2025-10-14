using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public int maxMonsters = 5;
    public float spawnInterval = 3f;
    public float spawnRadius = 5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            int currentMonsters = GameObject.FindGameObjectsWithTag("Monster").Length;

            if (currentMonsters < maxMonsters)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
                Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
