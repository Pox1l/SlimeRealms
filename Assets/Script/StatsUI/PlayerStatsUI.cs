using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    // Změna: Rozdělení na Melee a Ranged texty
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

        // Reset proměnných
        meleeDamageText = null; rangedDamageText = null;
        healthText = null; defenseText = null; staminaText = null;

        foreach (var t in allTexts)
        {
            switch (t.name)
            {
                // Změna: Hledáme specifické názvy pro Melee a Ranged
                case "MeleeDamageValue": meleeDamageText = t; break;
                case "RangedDamageValue": rangedDamageText = t; break;

                case "HealthValue": healthText = t; break;
                case "DefenseValue": defenseText = t; break;
                case "StaminaValue": staminaText = t; break;
            }
        }

        if (meleeDamageText != null && rangedDamageText != null && healthText != null && defenseText != null && staminaText != null)
        {
            Debug.Log("UI Texty pro statistiky úspěšně nalezeny.");
        }
        else
        {
            Debug.LogWarning("Nebyly nalezeny všechny UI Texty (hledám: MeleeDamageValue, RangedDamageValue, HealthValue, DefenseValue, StaminaValue).");
        }
    }

    public void UpdateStatsDisplay()
    {
        if (healthText == null || PlayerDataManager.Instance == null) return;

        PlayerData data = PlayerDataManager.Instance.currentData;

        // --- 🔥 VÝPOČET DAMAGE (Rozděleno) ---
        int displayMeleeDamage = 0;
        int displayRangedDamage = 0;

        float multiplier = PlayerStats.Instance != null ? PlayerStats.Instance.damageMultiplier : 1f;
        PlayerAttackSystem attackSystem = FindObjectOfType<PlayerAttackSystem>();

        if (attackSystem != null)
        {
            // ZDE JE POTŘEBA TVŮJ ZÁSAH: 
            // Protože 'attackSystem.currentAttack' je jen jedna aktivní zbraň, 
            // pro zobrazení obou hodnot najednou musíš mít v attackSystemu uložené odkazy na "vybavený meč" a "vybavený luk".

            // Příklad (pokud bys měl v AttackSystemu proměnné 'meleeWeapon' a 'rangedWeapon'):
            // if (attackSystem.meleeWeapon != null) 
            //    displayMeleeDamage = Mathf.RoundToInt(attackSystem.meleeWeapon.baseDamage * multiplier);

            // if (attackSystem.rangedWeapon != null) 
            //    displayRangedDamage = Mathf.RoundToInt(attackSystem.rangedWeapon.baseDamage * multiplier);

            // PROZATÍMNÍ LOGIKA (ukazuje currentAttack tam, kam patří):
            if (attackSystem.currentAttack != null)
            {
                float calculatedDmg = attackSystem.currentAttack.baseDamage * multiplier;

                // Jednoduchá detekce, zda je to melee nebo ranged (zde předpokládám tag nebo typ, uprav dle potřeby)
                // Pokud nemáš jak rozlišit, budeš muset upravit AttackSO script.
                // Pro teď to vypisuji do obou, dokud si to nenapojíš přesně:
                displayMeleeDamage = Mathf.RoundToInt(calculatedDmg);
                displayRangedDamage = Mathf.RoundToInt(calculatedDmg);
            }
        }

        // --- VÝPIS ---
        if (meleeDamageText != null) meleeDamageText.text = $"DMG {displayMeleeDamage}"; // Např. "DMG 10"
        if (rangedDamageText != null) rangedDamageText.text = $"DMG {displayRangedDamage}"; // Např. "DMG 20"

        healthText.text = $"Health: {data.maxHealth}";
        defenseText.text = $"Defense: {Mathf.RoundToInt(data.defense)}";
        staminaText.text = $"Stamina: {Mathf.RoundToInt(data.maxStamina)}";
    }
}