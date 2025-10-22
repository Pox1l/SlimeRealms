using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrystalUIController : MonoBehaviour
{
    [System.Serializable]
    public class Requirement
    {
        public ItemSO itemSO;
        public int requiredAmount;
    }

    [Header("References")]
    public InventoryManager inventoryManager; // 🟢 přidaná ruční reference
    public GameObject mainPanel;

    [Header("UI References")]
    public Transform requirementsParent;
    public GameObject requirementPrefab;
    public Button repairButton;

    [Header("Requirements")]
    public List<Requirement> requirements = new List<Requirement>();

    void OnEnable()
    {
        UpdateRequirements();
    }

    public void UpdateRequirements()
    {
        if (requirementsParent == null || requirementPrefab == null)
        {
            Debug.LogWarning("CrystalUIController: Missing UI references!");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogError("CrystalUIController: InventoryManager reference missing!");
            return;
        }

        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        bool canRepair = true;

        foreach (var req in requirements)
        {
            GameObject go = Instantiate(requirementPrefab, requirementsParent);
            go.transform.Find("Icon").GetComponent<Image>().sprite = req.itemSO.icon;
            go.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                $"{req.requiredAmount}x";

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount)
                canRepair = false;
        }

        repairButton.interactable = canRepair;
    }

    public void OnRepairPressed()
    {
        if (inventoryManager == null) return;

        foreach (var req in requirements)
        {
            inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);
        }

        Debug.Log(" Crystal repaired or upgraded!");
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClosePressed()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
