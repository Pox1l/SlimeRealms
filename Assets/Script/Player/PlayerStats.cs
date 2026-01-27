using UnityEngine;
using System;
using System.Collections; // 🔥 NUTNÉ PRO COROUTINES

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

    // Eventy
    public event Action<int, int> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;
    public static event Action<int> OnPlayerHit;

    [Header("Components")]
    public PlayerKnockback playerKnockback;
    public DamageFlash damageFlash;

    // 🔥 NOVÉ: Proměnná pro odložené ukládání
    private Coroutine saveCoroutine;

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
        RecalculateStats(false, false);

        if (data.currentHealth > 0) currentHealth = data.currentHealth;
        else currentHealth = maxHealth;

        currentStamina = maxStamina;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    // 🔥 Toto je klasické uložení (používáme při Heal, Upgrade, Start atd.)
    public void SaveToManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerStats(currentHealth, maxHealth, maxStamina, maxStamina, defense);
        }
    }

    // 🔥 NOVÉ: Odložené uložení (používáme v boji)
    public void RequestDelayedSave()
    {
        // Pokud už běží odpočet, zastavíme ho (resetujeme časovač)
        if (saveCoroutine != null) StopCoroutine(saveCoroutine);

        // Spustíme nový odpočet
        saveCoroutine = StartCoroutine(SaveAfterDelay());
    }

    // 🔥 NOVÉ: Samotný odpočet
    IEnumerator SaveAfterDelay()
    {
        // Počkáme 3 sekundy. Pokud během té doby dostaneš další hit,
        // tato coroutina se zruší a spustí se nová.
        yield return new WaitForSeconds(3f);

        SaveToManager(); // Teprve teď uložíme
        saveCoroutine = null;
        // Debug.Log("AutoSave Complete"); // Pro kontrolu můžeš odkomentovat
    }

    public void TakeDamage(int baseDamage, Transform attacker = null)
    {
        if (currentHealth <= 0) return;

        int finalDamage = baseDamage;

        if (!ignoreDefense)
        {
            finalDamage = baseDamage - Mathf.RoundToInt(defense);
        }

        finalDamage = Mathf.Max(1, finalDamage);

        // Debug.Log($"Enemy Dmg: {baseDamage} | Defense: {defense} | Final: {finalDamage}");

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        OnPlayerHit?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (attacker != null && playerKnockback != null) playerKnockback.ApplyKnockback(attacker);
        if (damageFlash != null) damageFlash.Flash();

        // 🔥 ZMĚNA: Místo SaveToManager() voláme DelayedSave
        RequestDelayedSave();

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        SaveToManager(); // U léčení to nevadí, to se neděje 10x za sekundu
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("💀 Player died!");

        // 1. Důležité: Pokud běží nějaký odpočet uložení, OKAMŽITĚ ho zruš.
        // Nechceme, aby se za 2 sekundy něco snažilo přepsat náš zápis smrti.
        if (saveCoroutine != null) StopCoroutine(saveCoroutine);

        // 2. Pro jistotu vynulujeme životy (kdyby náhodou byly třeba -5)
        currentHealth = 0;
        // Aktualizujeme UI, ať to vypadá hezky (0/100)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 3. 🔥 KLÍČOVÉ: Při smrti ukládáme HNED (žádný delay)!
        // Tím se do JSONu zapíše "currentHealth": 0
        SaveToManager();

        OnPlayerDied?.Invoke();
    }
}