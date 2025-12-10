using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public int baseMaxHealth = 100;
    public float baseMaxStamina = 100;

    [Header("Runtime Stats")]
    public int maxHealth;
    public int currentHealth;

    public float maxStamina;
    public float currentStamina;

    [Header("Stamina Settings")]
    public float staminaRegenRate = 15f; // 🔥 Přesunuto z PlayerMovement

    [Header("Defense")]
    public float baseDefense = 25f;
    public float defense;
    public bool ignoreDefense = false;

    public event Action<int, int> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        //RecalculateStats(false);
        LoadStateFromManager();
    }

    void Update()
    {
        // 🔥 REGENERACE STAMINY (Přesunuto sem)
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;

            // Event voláme jen při změně, ale neukládáme na disk každý frame (moc náročné)
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    // --- METODY PRO POHYB ---

    // Zkusí odečíst staminu. Vrací true, pokud na to hráč má.
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            SaveToManager(); // Uložíme změnu
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
                    }
                }
            }
        }

        maxHealth = Mathf.RoundToInt(calculatedHealth);
        maxStamina = calculatedStamina;
        defense = calculatedDefense;

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

            // 1. Načtení MAX hodnot (To ti funguje, vidím 160)
            maxHealth = data.maxHealth;
            maxStamina = data.maxStamina;
            defense = data.defense;

            // 2. 🔥 OPRAVA ZDE:
            // Místo načítání starých životů (data.currentHealth) je nastavíme rovnou na MAX.
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }
        else
        {
            // Fallback (kdyby neexistoval manager)
            currentHealth = baseMaxHealth;
            maxHealth = baseMaxHealth;
            currentStamina = baseMaxStamina;
            maxStamina = baseMaxStamina;
        }

        // Aktualizace UI hned po načtení
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

    public void TakeDamage(int baseDamage)
    {
        int finalDamage = baseDamage;
        if (!ignoreDefense)
        {
            float damageReduction = 100f / (100f + defense);
            finalDamage = Mathf.RoundToInt(baseDamage * damageReduction);
        }
        finalDamage = Mathf.Max(1, finalDamage);
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

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