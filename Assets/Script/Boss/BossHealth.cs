using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 500;
    public int currentHealth;

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    private EnemyDrop drop;
    private EnemyController controller;
    private DamageFlash damageFlash;
    private EnemyKnockback knockback;
    private BossEncounter bossEncounter;

    void Awake()
    {
        drop = GetComponent<EnemyDrop>();
        controller = GetComponent<EnemyController>();
        damageFlash = GetComponent<DamageFlash>();
        knockback = GetComponent<EnemyKnockback>();
    }

    void Start()
    {
        bossEncounter = FindObjectOfType<BossEncounter>();

        // --- PŘIDÁNO: Inicializace UI ---
        // Pokud je UI aktivní, nastavíme mu plné životy hned po spawnu
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);
        }
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

        // --- PŘIDÁNO: Aktualizace UI ---
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(currentHealth, maxHealth);
        }

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

        // --- PŘIDÁNO: Schování UI po smrti ---
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.HideUI();
        }

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

        OnDeath?.Invoke();
    }
}