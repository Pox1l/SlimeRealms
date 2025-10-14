using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;
    public float attackCooldown = 1f; // 1 sekunda mezi útoky

    private float lastAttackTime = -999f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryAttack(other);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryAttack(other);
        }
    }

    void TryAttack(Collider2D other)
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}
