using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FMODUnity; // 🔥 PŘIDÁNO: Knihovna pro FMOD

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI skillNameText;
    public Image selectedSkillImage;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueText;

    public Button purchaseButton;
    public TextMeshProUGUI purchaseButtonText;

    [Header("Resources Grid")]
    public Transform requirementsContainer;
    public GameObject requirementPrefab;

    [Header("Audio (FMOD)")] // 🔥 PŘIDÁNO: Sekce pro zvuk
    public EventReference upgradeSound; // Zvuk při úspěšném zakoupení/upgradu skillu

    private SkillSlot selectedSlot;
    private SkillSO selectedSkill => selectedSlot != null ? selectedSlot.skillData : null;

    public void SelectSkill(SkillSlot slot)
    {
        selectedSlot = slot;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (selectedSlot == null || selectedSkill == null) return;

        skillNameText.text = selectedSkill.skillName;
        if (selectedSkillImage != null) selectedSkillImage.sprite = selectedSkill.icon;

        // 1. Zobrazení popisu
        if (descriptionText != null) descriptionText.text = selectedSkill.description;

        // 2. Zobrazení hodnot (Value Text)
        if (valueText != null)
        {
            // Nyní pracujeme s INT (celá čísla)
            int currentVal = selectedSkill.GetTotalBonus();
            int nextVal = (selectedSkill.currentLevel + 1) * selectedSkill.valuePerLevel;

            // Zkratka podle typu skillu
            string unit = "";
            switch (selectedSkill.type)
            {
                case SkillType.Damage: unit = "DMG"; break;
                case SkillType.Health: unit = "HP"; break;
                case SkillType.Speed: unit = "SPD"; break;
                case SkillType.Defense: unit = "DEF"; break;
                case SkillType.Stamina: unit = "STM"; break;
                default: unit = ""; break;
            }

            // Odstraněna logika pro procenta, vypisujeme přímo číslo
            string curStr = $"{currentVal}";
            string nextStr = $"{nextVal}";

            // Výpis: "Aktuální -> Příští"
            if (selectedSkill.currentLevel < selectedSkill.MaxLevel)
            {
                valueText.text = $"{curStr} {unit} -> <color=#00FF00>{nextStr} {unit}</color>";
            }
            else
            {
                valueText.text = $"{curStr} {unit} <color=orange>(MAX)</color>";
            }
        }

        // Vyčistit grid
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);

        // Max Level kontrola
        if (selectedSkill.currentLevel >= selectedSkill.MaxLevel)
        {
            purchaseButtonText.text = "Max Level";
            purchaseButton.interactable = false;
        }
        else
        {
            // Zobrazení ceny
            List<Requirement> currentCost = selectedSkill.levels[selectedSkill.currentLevel].cost;
            bool canAfford = true;

            foreach (var req in currentCost)
            {
                GameObject obj = Instantiate(requirementPrefab, requirementsContainer);
                int playerHas = InventoryManager.Instance.GetTotalItemCount(req.item);

                var reqUI = obj.GetComponent<RequirementUI>();
                if (reqUI) reqUI.Setup(req.item.icon, playerHas, req.amount);

                if (playerHas < req.amount) canAfford = false;
            }

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(TryUpgrade);

            purchaseButtonText.text = "Upgrade";
            purchaseButton.interactable = canAfford;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(requirementsContainer.GetComponent<RectTransform>());
    }

    void TryUpgrade()
    {
        if (selectedSkill == null) return;

        bool success = SkillDatabase.Instance.TryUpgradeSkill(selectedSkill);

        if (success)
        {
            // --- 🔥 PŘIDÁNO: Přehrání zvuku při úspěšném nákupu ---
            if (!upgradeSound.IsNull)
            {
                RuntimeManager.PlayOneShot(upgradeSound);
            }
            // -------------------------------------------------------

            selectedSlot.UpdateSlotVisuals();
            UpdateUI();
        }
    }
}