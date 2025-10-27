using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Defense")]
    [Tooltip("Hodnota obrany – snižuje příchozí poškození")]
    public float defense = 25f;

    [Header("Ignore Defense")]
    [Tooltip("Pokud je zapnuto, obrana se při výpočtu dmg ignoruje (např. boss fight)")]
    public bool ignoreDefense = false;

    // Event pro UI
    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ================= HEALTH ==================
    public void TakeDamage(int baseDamage)
    {
        int finalDamage = baseDamage;

        // ✅ Pokud obrana není vypnutá, přepočti poškození
        if (!ignoreDefense)
        {
            float reduced = baseDamage * (100f / (100f + defense));
            finalDamage = Mathf.RoundToInt(reduced);
        }

        // ✅ Ujisti se, že hráč vždy dostane aspoň 1 dmg
        finalDamage = Mathf.Max(1, finalDamage);

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"💥 Player took {finalDamage} damage (base {baseDamage}, defense {defense})");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("💀 Player died!");
        // sem můžeš později přidat respawn, animaci, atd.
    }
}
