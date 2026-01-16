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
        // (Tato část zůstává stejná jako předtím, pro zkrácení ji vynechávám, ale v kódu ji nech)
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
        float multiplier = PlayerStats.Instance != null ? PlayerStats.Instance.damageMultiplier : 1f;
        PlayerAttackSwitcher switcher = FindObjectOfType<PlayerAttackSwitcher>();

        int displayMeleeDamage = 0;
        int displayRangedDamage = 0;

        // --- 🔥 PROHLEDÁNÍ SEZNAMU ZBRANÍ V SWITCHERU 🔥 ---
        if (switcher != null && switcher.availableAttacks != null)
        {
            foreach (AttackBase attack in switcher.availableAttacks)
            {
                if (attack == null) continue;

                // Zjistíme damage podle typu skriptu (MeleeAttack vs RangedAttack)
                int dmg = Mathf.CeilToInt(attack.baseDamage * multiplier);

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

        healthText.text = $"Health: {data.maxHealth}";
        defenseText.text = $"Defense: {Mathf.RoundToInt(data.defense)}";
        staminaText.text = $"Stamina: {Mathf.RoundToInt(data.maxStamina)}";
    }
}