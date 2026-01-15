using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI skillNameText;
    public Image selectedSkillImage;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueText; // PŘIDÁNO: Sem přetáhni nový Value Text (TMP)

    public Button purchaseButton;
    public TextMeshProUGUI purchaseButtonText;

    [Header("Resources Grid")]
    public Transform requirementsContainer;
    public GameObject requirementPrefab;

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

        // 1. Zobrazení popisu (pouze text)
        if (descriptionText != null) descriptionText.text = selectedSkill.description;

        // 2. Zobrazení hodnot (Value Text)
        if (valueText != null)
        {
            float currentVal = selectedSkill.GetTotalBonus();
            float nextVal = (selectedSkill.currentLevel + 1) * selectedSkill.valuePerLevel;

            // Pokud je hodnota malá (<= 1), bereme to jako procenta (např. 0.2 = 20%)
            bool isPercent = selectedSkill.valuePerLevel <= 1f;

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

            // Formátování čísel
            string curStr = isPercent ? $"{Mathf.Round(currentVal * 100)}%" : $"{currentVal}";
            string nextStr = isPercent ? $"{Mathf.Round(nextVal * 100)}%" : $"{nextVal}";

            // Výpis: "Aktuální -> Příští"
            if (selectedSkill.currentLevel < selectedSkill.MaxLevel)
            {
                // Příklad: 10% DMG -> 20% DMG (druhé číslo zeleně)
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

        // Refresh layoutu pro scrollbar
        LayoutRebuilder.ForceRebuildLayoutImmediate(requirementsContainer.GetComponent<RectTransform>());
    }

    void TryUpgrade()
    {
        if (selectedSkill == null) return;

        bool success = SkillDatabase.Instance.TryUpgradeSkill(selectedSkill);

        if (success)
        {
            selectedSlot.UpdateSlotVisuals();
            UpdateUI();
        }
    }
}