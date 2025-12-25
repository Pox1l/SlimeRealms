using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int baseMaxHealth = 100;
    public float baseMaxStamina = 100;
    public float baseDamageMultiplier = 1f; // 🆕 Základní poškození (1 = 100%)

    [Header("Runtime Stats")]
    public int maxHealth;
    public int currentHealth;

    public float maxStamina;
    public float currentStamina;

    // 🆕 Zde se uloží finální síla útoku (např. 1.5 pro +50% dmg)
    public float damageMultiplier = 1f;

    [Header("Stamina Settings")]
    public float staminaRegenRate = 15f;

    [Header("Defense")]
    public float baseDefense = 25f;
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
        //RecalculateStats(false);
        LoadStateFromManager();
    }

    void Update()
    {
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
            SaveToManager();
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    // --- ZBYTEK TVÉHO KÓDU (Recalculate, Save/Load, Damage...) ---

    public void RecalculateStats(bool healOnIncrease = true)
    {
        int oldMaxHealth = maxHealth;
        float oldMaxStamina = maxStamina;

        float calculatedHealth = baseMaxHealth;
        float calculatedStamina = baseMaxStamina;
        float calculatedDefense = baseDefense;
        float calculatedDamage = baseDamageMultiplier; // 🆕 Začínáme na 1.0

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
                        // 🆕 Započítání damage ze skillů
                        case SkillType.Damage: calculatedDamage += skill.GetTotalBonus(); break;
                    }
                }
            }
        }

        maxHealth = Mathf.RoundToInt(calculatedHealth);
        maxStamina = calculatedStamina;
        defense = calculatedDefense;
        damageMultiplier = calculatedDamage; // 🆕 Uložení výsledku

        // Doplnění po upgradu
        if (healOnIncrease)
        {
            if (maxHealth > oldMaxHealth) currentHealth += (maxHealth - oldMaxHealth);
            if (maxStamina > oldMaxStamina) currentStamina += (maxStamina - oldMaxStamina);
        }

        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentStamina > maxStamina) currentStamina = maxStamina;

        SaveToManager();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void LoadStateFromManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            var data = PlayerDataManager.Instance.currentData;

            maxHealth = data.maxHealth;
            maxStamina = data.maxStamina;
            defense = data.defense;

            // Poznámka: Damage se obvykle neukládá do SaveFile, 
            // protože se vždy přepočítá ze Skillů. Pokud ho tam chceš, přidej ho.
            // Pro jistotu zavoláme přepočet, aby se damage nastavil správně podle aktivních skillů:
            RecalculateStats(false);

            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }
        else
        {
            currentHealth = baseMaxHealth;
            maxHealth = baseMaxHealth;
            currentStamina = baseMaxStamina;
            maxStamina = baseMaxStamina;
            damageMultiplier = baseDamageMultiplier;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void SaveToManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerStats(currentHealth, maxHealth, currentStamina, maxStamina, defense);
        }
    }

    public void TakeDamage(int baseDamage, Transform attacker = null)
    {
        int finalDamage = baseDamage;
        if (!ignoreDefense)
        {
            float damageReduction = 100f / (100f + defense);
            finalDamage = Mathf.RoundToInt(baseDamage * damageReduction);
        }
        finalDamage = Mathf.Max(1, finalDamage);
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        if (attacker != null && playerKnockback != null)
        {
            playerKnockback.ApplyKnockback(attacker);
        }

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

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