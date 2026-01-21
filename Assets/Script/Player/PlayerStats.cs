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

    // Původní eventy
    public event Action<int, int> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;

    // 🔥 NOVÉ: Event pro Camera Shake / Feel (posílá, kolik dmg jsi dostal)
    public static event Action<int> OnPlayerHit;

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

        currentStamina = maxStamina;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void SaveToManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerStats(currentHealth, maxHealth, maxStamina, maxStamina, defense);
        }
    }

    public void TakeDamage(int baseDamage, Transform attacker = null)
    {
        int finalDamage = baseDamage;

        if (!ignoreDefense)
        {
            float damageMultiplier = 1f - defense;
            damageMultiplier = Mathf.Clamp(damageMultiplier, 0f, 1f);
            finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        }

        finalDamage = Mathf.Max(1, finalDamage);

        Debug.Log($"Enemy Dmg: {baseDamage} | Def Reduction: {defense * 100}% | Final: {finalDamage}");

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        // 🔥 NOVÉ: Zde voláme Feedback systém (Feel se o vše postará)
        OnPlayerHit?.Invoke(finalDamage);

        if (attacker != null && playerKnockback != null) playerKnockback.ApplyKnockback(attacker);
        if (damageFlash != null) damageFlash.Flash();

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