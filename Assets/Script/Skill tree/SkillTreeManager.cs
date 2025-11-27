using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI skillNameText;
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

        // Vyčistit grid
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);

        // Max Level kontrola
        if (selectedSkill.currentLevel >= selectedSkill.MaxLevel)
        {
            purchaseButtonText.text = "Max Level";
            purchaseButton.interactable = false;
            return;
        }

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

    void TryUpgrade()
    {
        if (selectedSkill == null) return;

        // Voláme databázi
        bool success = SkillDatabase.Instance.TryUpgradeSkill(selectedSkill);

        if (success)
        {
            selectedSlot.UpdateSlotVisuals(); // Update levého tlačítka
            UpdateUI(); // Update pravého panelu
        }
    }
}