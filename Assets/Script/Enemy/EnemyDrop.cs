using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject dropPrefab;
    public float dropChance = 1f; // 1 = 100%, 0.5 = 50%...

    private EnemyHealth health;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleDrop;
        }
    }

    void HandleDrop()
    {
        if (dropPrefab != null && Random.value <= dropChance)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }
}
