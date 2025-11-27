using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int baseMaxHealth = 100;

    [Header("Runtime Stats")]
    public int maxHealth;     // Vypočítané (Base + Skilly)
    public int currentHealth; // Aktuální stav

    [Header("Defense")]
    public float defense = 25f;
    public bool ignoreDefense = false;

    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Vypočítáme MAX HP (Ze skillů)
        RecalculateStats();

        // 2. Načteme AKTUÁLNÍ HP (Z PlayerData)
        LoadHealthState();
    }

    public void RecalculateStats()
    {
        // --- VÝPOČET MAX HP (Skill Database) ---
        float calculatedHealth = baseMaxHealth;

        if (SkillDatabase.Instance != null)
        {
            foreach (var skill in SkillDatabase.Instance.allSkills)
            {
                if (skill.currentLevel > 0 && skill.type == SkillType.Health)
                {
                    calculatedHealth += skill.GetTotalBonus();
                }
            }
        }
        maxHealth = Mathf.RoundToInt(calculatedHealth);

        // Ošetření, aby current nepřesáhl max
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void LoadHealthState()
    {
        // --- NAČTENÍ AKTUÁLNÍHO HP (Player Data) ---
        if (PlayerDataManager.Instance != null)
        {
            int savedHP = PlayerDataManager.Instance.currentData.currentHealth;

            if (savedHP == -1) // -1 znamená "Nová hra" nebo "Reset"
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Clamp(savedHP, 0, maxHealth);
            }
        }
        else
        {
            currentHealth = maxHealth; // Fallback, kdyby manager neexistoval
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int baseDamage)
    {
        int finalDamage = baseDamage;

        if (!ignoreDefense)
        {
            float reduced = baseDamage * (100f / (100f + defense));
            finalDamage = Mathf.RoundToInt(reduced);
        }

        finalDamage = Mathf.Max(1, finalDamage);
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        // 🔥 ULOŽIT ZMĚNU DO PLAYER DATA
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerData(currentHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // 🔥 ULOŽIT ZMĚNU
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerData(currentHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("💀 Player died!");
    }
}