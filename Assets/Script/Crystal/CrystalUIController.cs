using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CrystalUIController : MonoBehaviour
{
    [System.Serializable]
    public class Requirement
    {
        public ItemSO itemSO;
        public int requiredAmount;
    }

    [System.Serializable]
    public class CrystalStage
    {
        public string stageName = "Stage";
        public List<Requirement> requirements = new List<Requirement>();

        // TLAČÍTKA SVĚTŮ, které se po opravě této stage povolí
        public List<Button> worldButtonsToEnable = new List<Button>();
    }

    [Header("References")]
    public InventoryManager inventoryManager;
    public GameObject mainPanel;

    [Header("UI – požadavky")]
    public Transform requirementsParent;
    public GameObject requirementPrefab;
    public Button repairButton;

    [Header("Stages")]
    public List<CrystalStage> stages = new List<CrystalStage>();

    int currentStage = 0;

    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        LockAllWorldButtons();
        RefreshStageUI();
    }

    void LockAllWorldButtons()
    {
        foreach (var stage in stages)
        {
            foreach (var btn in stage.worldButtonsToEnable)
            {
                if (btn != null)
                    btn.interactable = false;
            }
        }
    }

    public void OpenUI()
    {
        mainPanel.SetActive(true);
        Time.timeScale = 0;
        RefreshStageUI();
    }

    public void CloseUI()
    {
        mainPanel.SetActive(false);
        Time.timeScale = 1;
    }

    void RefreshStageUI()
    {
        if (currentStage >= stages.Count)
        {
            repairButton.interactable = false;
            return;
        }

        var stage = stages[currentStage];

        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        bool canRepair = true;

        foreach (var req in stage.requirements)
        {
            GameObject row = Instantiate(requirementPrefab, requirementsParent);
            row.transform.Find("Icon").GetComponent<Image>().sprite = req.itemSO.icon;
            row.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                $"{req.requiredAmount}x";

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount)
                canRepair = false;
        }

        repairButton.interactable = canRepair;
    }

    public void OnRepairPressed()
    {
        if (currentStage >= stages.Count)
            return;

        var stage = stages[currentStage];

        // kontrola itemů
        foreach (var req in stage.requirements)
        {
            if (inventoryManager.GetTotalItemCount(req.itemSO) < req.requiredAmount)
            {
                RefreshStageUI();
                return;
            }
        }

        // odeber itemy
        foreach (var req in stage.requirements)
            inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);

        // povol tlačítka světů
        foreach (var btn in stage.worldButtonsToEnable)
        {
            if (btn != null)
                btn.interactable = true;
        }

        // další stage
        currentStage++;
        RefreshStageUI();
    }
}
