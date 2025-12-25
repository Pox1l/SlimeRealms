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
        // Nejdřív načteme data, to si samo zavolá i přepočet statistik
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

    public void RecalculateStats(bool healOnIncrease = true, bool autoSave = true)
    {
        // 1. Uložíme si staré hodnoty, abychom věděli, o kolik se zvedly
        int oldMaxHealth = maxHealth;
        float oldMaxStamina = maxStamina;

        // 2. Resetujeme na základní hodnoty (Base Stats)
        float calculatedHealth = baseMaxHealth;
        float calculatedStamina = baseMaxStamina;
        float calculatedDefense = baseDefense;
        float calculatedDamage = baseDamageMultiplier;

        // 3. Projdeme všechny skilly a přičteme bonusy
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

        // 4. Aplikujeme vypočítané hodnoty do hlavních proměnných
        maxHealth = Mathf.RoundToInt(calculatedHealth);
        maxStamina = calculatedStamina;
        defense = calculatedDefense;
        damageMultiplier = calculatedDamage;

        // 5. Pokud se zvýšilo MAX HP/Stamina, přidáme ten rozdíl i do aktuálního (léčení při level upu)
        if (healOnIncrease)
        {
            if (maxHealth > oldMaxHealth) currentHealth += (maxHealth - oldMaxHealth);
            if (maxStamina > oldMaxStamina) currentStamina += (maxStamina - oldMaxStamina);
        }

        // 6. Ořezání - aktuální zdraví nesmí být vyšší než maximální
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentStamina > maxStamina) currentStamina = maxStamina;

        // 7. 🔥 KLÍČOVÁ ČÁST: Ukládáme jen pokud je autoSave zapnuté
        // (Při načítání hry sem pošleš false, takže se nepřepíše save file)
        if (autoSave)
        {
            SaveToManager();
        }

        // 8. Aktualizace UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void LoadStateFromManager()
    {
        // Pojistka: Pokud Manager nebo data neexistují, použijeme základy a skončíme
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentData == null)
        {
            Debug.LogWarning("PlayerDataManager chybí nebo nemá data. Používám defaultní stats.");
            RecalculateStats(false);
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            return;
        }

        var data = PlayerDataManager.Instance.currentData;

        // 1. Nejdřív přepočítáme MAX staty podle skillů (aby seděly levely)
        RecalculateStats(false,false); 

        // 2. Teď načteme AKTUÁLNÍ hodnoty ze save file (ne max!)
        // Pokud je v savu HP > 0, použijeme ho. Pokud je 0 nebo -1 (nová hra/chyba), dáme Max.
        if (data.currentHealth > 0) 
        {
            currentHealth = data.currentHealth;
        }
        else 
        {
            currentHealth = maxHealth;
        }

        // To samé pro staminu
        if (data.currentStamina > 0)
        {
            currentStamina = data.currentStamina;
        }
        else
        {
            currentStamina = maxStamina;
        }
        
        // Načtení obrany (pokud ji chceme brát ze savu, ale Recalculate ji už nastavil ze skillů.
        // Záleží, jestli se defense mění jen skilly (pak nechat Recalculate) nebo i itemy (pak načíst).
        // Pokud je defense čistě ze skillů, tento řádek smaž:
        // defense = data.defense; 

        // Zajistíme, že aktuální zdraví nepřetéká přes max (kdyby se změnily skilly/patche)
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

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