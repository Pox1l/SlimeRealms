using UnityEngine;
using System;
using FMODUnity; // <--- Důležité: Přidat knihovnu

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Audio")]
    [Tooltip("Pokud necháš prázdné, zahraje se Default Hit z Manageru")]
    public EventReference hitSound; // <--- Políčko pro custom zvuk

    public event Action OnDeath;

    private EnemyDrop drop;
    private EnemyController controller;
    private DamageFlash damageFlash;
    private EnemyKnockback knockback;

    void Awake()
    {
        drop = GetComponent<EnemyDrop>();
        controller = GetComponent<EnemyController>();
        damageFlash = GetComponent<DamageFlash>();
        knockback = GetComponent<EnemyKnockback>();
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (AudioManager.instance != null)
        {
            // 🔥 ZMĚNA: Posíláme i transform.position
            AudioManager.instance.PlayHitSound(hitSound, transform.position);
        }
        // -------------------------------

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (knockback != null)
        {
            knockback.PlayKnockback();
        }

        if (currentHealth <= 0)
        {
            if (drop != null) drop.DropLoot();
            OnDeath?.Invoke();
        }
        else
        {
            if (controller != null)
            {
                controller.OnHitAggro();
            }
        }
    }
}