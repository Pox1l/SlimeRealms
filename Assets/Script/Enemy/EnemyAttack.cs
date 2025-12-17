using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;
    public float attackCooldown = 1f;

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
                // 🔥 ZMĚNA ZDE: Přidali jsme ", transform"
                // Tím říkáme hráči: "Dostals dmg a útočník jsem JÁ (tento objekt)"
                playerStats.TakeDamage(damage, transform);

                lastAttackTime = Time.time;
            }
        }
    }
}