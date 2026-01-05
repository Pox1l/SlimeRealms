using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int baseMaxHealth = 100;
    public float baseMaxStamina = 100;
    public float baseDamageMultiplier = 1f;

    [Header("Runtime Stats")]
    public int maxHealth;
    public int currentHealth;

    public float maxStamina;
    public float currentStamina;

    public float damageMultiplier = 1f;

    [Header("Stamina Settings")]
    public float staminaRegenRate = 15f;

    [Header("Defense")]
    public float baseDefense = 0f;
    public float defense;
    public bool ignoreDefense = false;

    public event Action<int, int> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;

    [Header("Components")]
    public PlayerKnockback playerKnockback;
    public DamageFlash damageFlash;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (playerKnockback == null) playerKnockback = GetComponent<PlayerKnockback>();
        if (damageFlash == null) damageFlash = GetComponentInChildren<DamageFlash>();
    }

    void Start()
    {
        LoadStateFromManager();
    }

    void Update()
    {
        // Regenerace staminy
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    // --- METODY PRO POHYB ---
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;

            // ❌ ODSTRANĚNO: SaveToManager(); 
            // Důvod: Zápis na disk při každém sprintu způsobuje lagy.

            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void RecalculateStats(bool healOnIncrease = true, bool autoSave = true)
    {
        int oldMaxHealth = maxHealth;
        // float oldMaxStamina = maxStamina; // Už nepotřebujeme pro léčení, stamina se stejně doplní

        // Reset na základy
        float calculatedHealth = baseMaxHealth;
        float calculatedStamina = baseMaxStamina;
        float calculatedDefense = baseDefense;
        float calculatedDamage = baseDamageMultiplier;

        // Skilly
        if (SkillDatabase.Instance != null)
        {
            foreach (var skill in SkillDatabase.Instance.allSkills)
            {
                if (skill.currentLevel > 0)
                {
                    switch (skill.type)
                    {
                        case SkillType.Health: calculatedHealth += skill.GetTotalBonus(); break;
                        case SkillType.Stamina: calculatedStamina += skill.GetTotalBonus(); break;
                        case SkillType.Defense: calculatedDefense += skill.GetTotalBonus(); break;
                        case SkillType.Damage: calculatedDamage += skill.GetTotalBonus(); break;
                    }
                }
            }
        }

        maxHealth = Mathf.RoundToInt(calculatedHealth);
        maxStamina = calculatedStamina;
        defense = calculatedDefense;
        damageMultiplier = calculatedDamage;

        if (healOnIncrease)
        {
            if (maxHealth > oldMaxHealth) currentHealth += (maxHealth - oldMaxHealth);
        }

        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // 🔥 Při přepočtu statů se stamina vždy doplní, pokud chceme, nebo se jen ořízne
        if (currentStamina > maxStamina) currentStamina = maxStamina;

        if (autoSave)
        {
            SaveToManager();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void LoadStateFromManager()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentData == null)
        {
            RecalculateStats(false);
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            return;
        }

        var data = PlayerDataManager.Instance.currentData;

        // 1. Přepočet MAX hodnot
        RecalculateStats(false, false);

        // 2. Načtení HP
        if (data.currentHealth > 0)
        {
            currentHealth = data.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        // 🔥 ZMĚNA: Staminu nečteme ze savu, ale vždy ji dáme plnou
        currentStamina = maxStamina;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void SaveToManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            // Místo currentStamina tam posíláme maxStamina (nebo cokoliv, stejně to nebudeme načítat)
            PlayerDataManager.Instance.SavePlayerStats(currentHealth, maxHealth, maxStamina, maxStamina, defense);
        }
    }

    // 🔥 TOTO JE UPRAVENÁ METODA PRO DMG
    public void TakeDamage(int baseDamage, Transform attacker = null)
    {
        int finalDamage = baseDamage;

        if (!ignoreDefense)
        {
            // Jednoduché odečtení: Útok - Obrana
            // Příklad: 10 dmg - 4 def = 6 dmg
            // Příklad: 10 dmg - 25 def = -15 dmg (pořešíme níže)
            finalDamage = baseDamage - (int)defense;
        }

        // 🔥 Pojistka: Vždy udělíme alespoň 1 DMG (pokud chceš být nesmrtelný, dej sem 0)
        finalDamage = Mathf.Max(1, finalDamage);

        // Debug výpis pro kontrolu
        Debug.Log($"Enemy Dmg: {baseDamage} | Tvoje Def: {defense} | Finální Dmg: {finalDamage}");

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        if (attacker != null && playerKnockback != null)
        {
            playerKnockback.ApplyKnockback(attacker);
        }

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        // Tady SaveToManager necháme, protože poškození se nestává 60x za vteřinu
        SaveToManager();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        SaveToManager();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("💀 Player died!");
        OnPlayerDied?.Invoke();
    }
}