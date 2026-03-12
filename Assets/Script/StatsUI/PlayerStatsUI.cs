using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    public TextMeshProUGUI meleeDamageText;
    public TextMeshProUGUI rangedDamageText;

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

        meleeDamageText = null; rangedDamageText = null;
        healthText = null; defenseText = null; staminaText = null;

        foreach (var t in allTexts)
        {
            switch (t.name)
            {
                case "MeleeDamageValue": meleeDamageText = t; break;
                case "RangedDamageValue": rangedDamageText = t; break;
                case "HealthValue": healthText = t; break;
                case "DefenseValue": defenseText = t; break;
                case "StaminaValue": staminaText = t; break;
            }
        }
    }

    public void UpdateStatsDisplay()
    {
        if (healthText == null || PlayerDataManager.Instance == null) return;

        PlayerData data = PlayerDataManager.Instance.currentData;

        // --- Získání referencí ---
        // 🔥 ZMĚNA: Převedeme hodnotu na int. Předpokládáme, že 'damageMultiplier' 
        // nyní v PlayerStats funguje jako sčítač bonusů (např. hodnota 2, 5, 10).
        int damageBonus = PlayerStats.Instance != null ? (int)PlayerStats.Instance.damageMultiplier : 0;

        // POZNÁMKA: Pokud PlayerStats stále začíná na 1 (kvůli násobení), 
        // budeš tam muset odečíst 1, nebo v PlayerStats nastavit start na 0.

        PlayerAttackSwitcher switcher = FindObjectOfType<PlayerAttackSwitcher>();

        int displayMeleeDamage = 0;
        int displayRangedDamage = 0;

        // --- 🔥 PROHLEDÁNÍ SEZNAMU ZBRANÍ V SWITCHERU 🔥 ---
        if (switcher != null && switcher.availableAttacks != null)
        {
            foreach (AttackBase attack in switcher.availableAttacks)
            {
                if (attack == null) continue;

                // 🔥 OPRAVA: Změněno z násobení (*) na sčítání (+)
                // Protože jsme přešli na celá čísla (Flat Bonus), damage se prostě přičte.
                // Příklad: Base 10 + Bonus 1 = 11.
                int dmg = attack.baseDamage + damageBonus;

                if (attack is MeleeAttack)
                {
                    displayMeleeDamage = dmg;
                }
                else if (attack is RangedAttack)
                {
                    displayRangedDamage = dmg;
                }
            }
        }

        // --- VÝPIS DO UI ---
        if (meleeDamageText != null) meleeDamageText.text = $"DMG {displayMeleeDamage}";
        if (rangedDamageText != null) rangedDamageText.text = $"DMG {displayRangedDamage}";

        healthText.text = $"Max Health: {data.maxHealth}";
        defenseText.text = $"Max Defense: {Mathf.RoundToInt(data.defense)}";
        staminaText.text = $"Max Stamina: {Mathf.RoundToInt(data.maxStamina)}";
    }
}