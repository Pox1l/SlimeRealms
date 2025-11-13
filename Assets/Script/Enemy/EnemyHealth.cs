using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    public event Action OnDeath;

    private EnemyDrop drop;

    void Awake()
    {
        drop = GetComponent<EnemyDrop>();
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // už je mrtvý, ignoruj další dmg

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // 💥 Drop JEDNOU
            if (drop != null)
                drop.DropLoot();

            // 🔔 Dá signál “umřel”
            OnDeath?.Invoke();
        }
    }
}
