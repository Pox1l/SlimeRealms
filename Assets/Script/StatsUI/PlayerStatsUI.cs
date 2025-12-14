using UnityEngine;
using TMPro;
using System.Linq;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    private TextMeshProUGUI damageText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI defenseText;
    private TextMeshProUGUI staminaText;

    private const int DEFAULT_DAMAGE_PLACEHOLDER = 50;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. Najít texty (i když jsou vypnuté), přímo pod tímto objektem ("Stats")
        FindUITextsInScene();

        // 2. Aktualizovat obsah
        UpdateStatsDisplay();
    }

    // ----------------------------------------------------
    // --- METODA PRO HLEDÁNÍ A PROPOJOVÁNÍ ---
    // ----------------------------------------------------

    public void FindUITextsInScene()
    {
        // 🔥 Nová, správná logika: Získat všechny Texty POUZE POD TÍMTO OBJEKTEM
        // GetComponentsInChildren(true) zajistí, že se hledají i neaktivní děti
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        // Reset referencí
        damageText = null; healthText = null; defenseText = null; staminaText = null;

        foreach (var t in allTexts)
        {
            switch (t.name)
            {
                // Jména dětí objektu Stats: DamageValue, HealthValue, atd.
                case "DamageValue":
                    damageText = t;
                    break;
                case "HealthValue":
                    healthText = t;
                    break;
                case "DefenseValue":
                    defenseText = t;
                    break;
                case "StaminaValue":
                    staminaText = t;
                    break;
            }
        }

        // Finalní kontrola
        if (damageText != null && healthText != null && defenseText != null && staminaText != null)
        {
            Debug.Log("UI Texty pro statistiky úspěšně nalezeny a propojeny.");
        }
        else
        {
            Debug.LogWarning("Nebyly nalezeny všechny 4 UI Texty pod objektem 'Stats'! Zkontrolujte, zda jsou pojmenovány DamageValue, HealthValue, DefenseValue, StaminaValue.");
        }
    }

    // ----------------------------------------------------
    // --- METODA PRO AKTUALIZACI ZOBRAZENÍ Z JSON DAT ---
    // ----------------------------------------------------

    public void UpdateStatsDisplay()
    {
        if (healthText == null || PlayerDataManager.Instance == null) return;

        PlayerData data = PlayerDataManager.Instance.currentData;

        damageText.text = $"Damage: {DEFAULT_DAMAGE_PLACEHOLDER}";
        healthText.text = $"Health: {data.maxHealth}";
        defenseText.text = $"Defense: {Mathf.RoundToInt(data.defense)}";
        staminaText.text = $"Stamina: {Mathf.RoundToInt(data.maxStamina)}";
    }
}