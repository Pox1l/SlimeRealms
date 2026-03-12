using UnityEngine;
using System;
using System.Collections;
using FMODUnity;

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

    [Header("Audio")]
    [Tooltip("Pokud necháš prázdné, zahraje se Default Player Hit z Manageru")]
    public EventReference hurtSFX;
    
    // Eventy
    public event Action<int, int> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;
    public static event Action<int> OnPlayerHit;

    [Header("Components")]
    public PlayerKnockback playerKnockback;
    public DamageFlash damageFlash;

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
            currentStamina = (float)Math.Round(currentStamina, 2);
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            currentStamina = (float)Math.Round(currentStamina, 2);
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

        if (autoSave) SaveToManager();

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

        if (data.currentHealth > 0)
        {
            currentHealth = data.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
            SaveToManager();
        }

        currentStamina = maxStamina;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void SaveToManager()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerStats(currentHealth, maxHealth, maxStamina, maxStamina, defense);
        }
    }

    public void RequestDelayedSave()
    {
        if (saveCoroutine != null) StopCoroutine(saveCoroutine);
        saveCoroutine = StartCoroutine(SaveAfterDelay());
    }

    IEnumerator SaveAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SaveToManager();
        saveCoroutine = null;
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

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        // 🔥 PŘIDÁNO: Volání AudioManageru pro zvuk zranění
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPlayerHitSound(hurtSFX, transform.position);
        }

        OnPlayerHit?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (attacker != null && playerKnockback != null) playerKnockback.ApplyKnockback(attacker);
        if (damageFlash != null) damageFlash.Flash();

        RequestDelayedSave();

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

        if (saveCoroutine != null) StopCoroutine(saveCoroutine);

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        SaveToManager();

        OnPlayerDied?.Invoke();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathScreen();
        }
    }
}