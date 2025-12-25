using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    public TextMeshProUGUI damageText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI staminaText;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        FindUITextsInScene();
        UpdateStatsDisplay();
    }

    public void FindUITextsInScene()
    {
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        damageText = null; healthText = null; defenseText = null; staminaText = null;

        foreach (var t in allTexts)
        {
            switch (t.name)
            {
                case "DamageValue": damageText = t; break;
                case "HealthValue": healthText = t; break;
                case "DefenseValue": defenseText = t; break;
                case "StaminaValue": staminaText = t; break;
            }
        }

        if (damageText != null && healthText != null && defenseText != null && staminaText != null)
        {
            Debug.Log("UI Texty pro statistiky úspěšně nalezeny.");
        }
        else
        {
            Debug.LogWarning("Nebyly nalezeny všechny UI Texty (hledám: DamageValue, HealthValue, DefenseValue, StaminaValue).");
        }
    }

    public void UpdateStatsDisplay()
    {
        // Kontrola, zda máme data
        if (healthText == null || PlayerDataManager.Instance == null) return;

        // Načtení uložených dat (HP, Stamina, Def)
        PlayerData data = PlayerDataManager.Instance.currentData;


        // --- 🔥 VÝPOČET DAMAGE (Nový kód) ---
        int displayDamage = 0;

        // 1. Získáme aktuální multiplier (bonus ze skill tree)
        // Pokud je PlayerStats načtený, vezmeme ho odtud. Jinak 1.0 (základ).
        float multiplier = PlayerStats.Instance != null ? PlayerStats.Instance.damageMultiplier : 1f;

        // 2. Najdeme AttackSystem na hráči, abychom zjistili Base Damage zbraně
        PlayerAttackSystem attackSystem = FindObjectOfType<PlayerAttackSystem>();

        if (attackSystem != null && attackSystem.currentAttack != null)
        {
            // Vzorec: Základ zbraně * Skill Bonus
            float calculatedDmg = attackSystem.currentAttack.baseDamage * multiplier;
            displayDamage = Mathf.RoundToInt(calculatedDmg);
        }
        else
        {
            // Fallback: Pokud hráč nemá zbraň nebo script, ukážeme 0 nebo nějaký základ
            displayDamage = 0;
        }

        // --- VÝPIS ---
        damageText.text = $"Damage: {displayDamage}"; // Např. "Damage: 15"

        healthText.text = $"Health: {data.maxHealth}";
        defenseText.text = $"Defense: {Mathf.RoundToInt(data.defense)}";
        staminaText.text = $"Stamina: {Mathf.RoundToInt(data.maxStamina)}";
    }
}