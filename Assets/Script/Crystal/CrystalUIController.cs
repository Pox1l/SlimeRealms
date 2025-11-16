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
        public List<Button> unlockedWorldButtons = new List<Button>();
    }

    [Header("References")]
    public InventoryManager inventoryManager;
    public GameObject mainPanel;

    [Header("UI")]
    public Transform requirementsParent;
    public GameObject requirementPrefab;
    public Button repairButton;

    [Header("Stages (0 = první, 1 = druhá, atd.)")]
    public List<CrystalStage> stages = new List<CrystalStage>();

    // NESERIALIZOVAT, ať s tím nic nehýbe v Inspectoru
    int currentStage = 0;

    void Start()
    {
        LockAllWorldButtons();
    }


    void LockAllWorldButtons()
    {
        if (stages == null) return;

        foreach (var stage in stages)
        {
            if (stage == null || stage.unlockedWorldButtons == null) continue;

            foreach (var btn in stage.unlockedWorldButtons)
            {
                if (btn != null)
                    btn.interactable = false;          // nebo btn.gameObject.SetActive(false);
            }
        }
    }


    void OnEnable()
    {
        RefreshStageUI();
    }

    public void OpenUI()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);

        Time.timeScale = 0f;
        RefreshStageUI();
    }

    void RefreshStageUI()
    {
        int count = stages == null ? 0 : stages.Count;
        Debug.Log($"[CrystalUI] RefreshStageUI: stages.Count={count}, currentStage={currentStage}");

        if (count == 0)
        {
            Debug.LogWarning("[CrystalUI] Žádné stages nejsou nastavené.");
            if (repairButton != null) repairButton.interactable = false;
            return;
        }

        if (currentStage >= count)
        {
            // jsme za posledním stage → krystal je full
            foreach (Transform child in requirementsParent)
                Destroy(child.gameObject);

            if (repairButton != null) repairButton.interactable = false;

            Debug.Log("[CrystalUI] Krystal je už na maximálním levelu.");
            return;
        }

        var stage = stages[currentStage];

        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        bool canRepair = true;

        foreach (var req in stage.requirements)
        {
            GameObject go = Instantiate(requirementPrefab, requirementsParent);
            go.transform.Find("Icon").GetComponent<Image>().sprite = req.itemSO.icon;
            go.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                $"{req.requiredAmount}x";

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount)
                canRepair = false;
        }

        if (repairButton != null)
            repairButton.interactable = canRepair;
    }

    public void OnRepairPressed()
    {
        int count = stages == null ? 0 : stages.Count;
        Debug.Log($"[CrystalUI] OnRepairPressed BEFORE: currentStage={currentStage}, stages.Count={count}");

        if (count == 0 || currentStage >= count)
        {
            Debug.LogWarning("[CrystalUI] OnRepairPressed volán mimo rozsah stages.");
            return;
        }

        var stage = stages[currentStage];

        // finální kontrola inventáře
        foreach (var req in stage.requirements)
        {
            if (inventoryManager.GetTotalItemCount(req.itemSO) < req.requiredAmount)
            {
                Debug.LogWarning("[CrystalUI] Player nemá požadované itemy (kontrola před odebráním).");
                RefreshStageUI();
                return;
            }
        }

        // odebrat itemy
        foreach (var req in stage.requirements)
        {
            inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);
        }

        // odemknout světy v tomhle stage
        foreach (var btn in stage.unlockedWorldButtons)
        {
            if (btn != null)
                btn.interactable = true;
        }

        Debug.Log($"[CrystalUI] Crystal repaired → stage {currentStage}");

        currentStage++;
        Debug.Log($"[CrystalUI] AFTER repair currentStage={currentStage}");

        OnClosePressed();
    }

    public void OnClosePressed()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
