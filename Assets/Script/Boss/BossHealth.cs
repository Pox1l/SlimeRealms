using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 500;
    public int currentHealth;

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    // Komponenty
    private EnemyDrop drop;
    private EnemyController controller;
    private DamageFlash damageFlash;
    private EnemyKnockback knockback;

    // ❌ SMAZÁNO: private ReturnToPoolOnDeath returnToPool; 
    // Už to nepotřebujeme, protože ReturnToPoolBoss poslouchá event OnDeath.

    // Odkaz na Spawner
    private BossEncounter bossEncounter;

    void Awake()
    {
        drop = GetComponent<EnemyDrop>();
        controller = GetComponent<EnemyController>();
        damageFlash = GetComponent<DamageFlash>();
        knockback = GetComponent<EnemyKnockback>();

        // ❌ SMAZÁNO: returnToPool = GetComponent<ReturnToPoolOnDeath>();
    }

    void Start()
    {
        bossEncounter = FindObjectOfType<BossEncounter>();
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (damageFlash != null) damageFlash.Flash();
        if (knockback != null) knockback.PlayKnockback();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (controller != null) controller.OnHitAggro();
        }
    }

    void Die()
    {
        Debug.Log("💀 Boss defeated!");

        if (drop != null) drop.DropLoot();

        if (bossEncounter != null)
        {
            bossEncounter.SetBossDefeated();
        }
        else
        {
            bossEncounter = FindObjectOfType<BossEncounter>();
            if (bossEncounter != null) bossEncounter.SetBossDefeated();
        }

        // ✅ TADY SE STANE KOUZLO:
        // Jen řekneme "Umřel jsem". Skript ReturnToPoolBoss to uslyší, 
        // počká 3 vteřiny a pak zavolá ForceReturn sám.
        OnDeath?.Invoke();

        // ❌ SMAZÁNO: Celý ten blok s returnToPool.Return() nebo ForceReturn().
        // Kdybys to tu nechal, boss by zmizel hned a nestihla by se animace.
    }
}