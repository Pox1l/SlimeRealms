using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    // Event pøi smrti
    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
