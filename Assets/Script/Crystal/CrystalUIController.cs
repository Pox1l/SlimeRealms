using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class CrystalUIController : MonoBehaviour
{
    // --- DATOVÉ TŘÍDY (Stejný styl jako InventorySlotData) ---

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

    [System.Serializable]
    public class CrystalSaveData
    {
        public int savedStageIndex;
    }

    // --- REFERENCE ---

    [Header("References")]
    public InventoryManager inventoryManager;
    public CrystalVisualController visualController; // 🔥 Vizuál
    public GameObject mainPanel;

    [Header("UI – požadavky")]
    public Transform requirementsParent;
    public GameObject requirementPrefab;
    public Button repairButton;

    [Header("Stages")]
    public List<CrystalStage> stages = new List<CrystalStage>();

    // --- PROMĚNNÉ ---

    private int currentStage = 0;
    private string saveFilePath;

    // --- HLAVNÍ METODY (Podle vzoru InventorySaveSystem) ---

    void Awake()
    {
        // 1. Nastavení cesty (stejně jako v InventorySaveSystem)
        saveFilePath = Path.Combine(Application.persistentDataPath, "crystal_save.json");
    }

    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        // 2. Automatické načtení při startu
        LoadCrystalData();

        // 3. Inicializace UI podle načtených dat
        LockAllWorldButtons();
        UnlockCompletedStages();

        // Nastavení vizuálu hned po startu
        if (visualController != null) visualController.UpdateVisuals(currentStage);

        RefreshStageUI();
    }

    void Update()
    {
        // 🛠 Debug klávesy jako v InventorySaveSystem
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveCrystalData();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadCrystalData();
            RefreshStageUI();
            if (visualController != null) visualController.UpdateVisuals(currentStage);
        }
    }

    // 4. Automatické uložení při vypnutí hry
    private void OnApplicationQuit()
    {
        SaveCrystalData();
    }

    // --- SAVE & LOAD SYSTÉM (Kopie stylu Inventory) ---

    public void SaveCrystalData()
    {
        CrystalSaveData data = new CrystalSaveData();
        data.savedStageIndex = currentStage;

        // true = hezké formátování JSONu (stejně jako v inventáři)
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"💾 Crystal saved to {saveFilePath}");
    }

    public void LoadCrystalData()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No crystal save found (New Game).");
            currentStage = 0;
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        CrystalSaveData data = JsonUtility.FromJson<CrystalSaveData>(json);

        currentStage = data.savedStageIndex;

        Debug.Log("📦 Crystal loaded! Stage: " + currentStage);
    }

    // --- UI LOGIKA (Původní funkčnost zachována) ---

    void LockAllWorldButtons()
    {
        foreach (var stage in stages)
        {
            foreach (var btn in stage.worldButtonsToEnable)
            {
                if (btn != null) btn.interactable = false;
            }
        }
    }

    void UnlockCompletedStages()
    {
        for (int i = 0; i < currentStage; i++)
        {
            if (i < stages.Count)
            {
                foreach (var btn in stages[i].worldButtonsToEnable)
                {
                    if (btn != null) btn.interactable = true;
                }
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
            foreach (Transform child in requirementsParent) Destroy(child.gameObject);
            if (repairButton != null)
            {
                repairButton.interactable = false;
                var txt = repairButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = "Completed";
            }
            return;
        }

        var stage = stages[currentStage];

        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        bool canRepair = true;

        foreach (var req in stage.requirements)
        {
            GameObject row = Instantiate(requirementPrefab, requirementsParent);

            Image iconImg = row.transform.Find("Icon").GetComponent<Image>();
            if (iconImg != null) iconImg.sprite = req.itemSO.icon;

            TextMeshProUGUI amountText = row.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            if (amountText != null) amountText.text = $"{req.requiredAmount}x";

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount)
            {
                canRepair = false;
                if (amountText != null) amountText.color = Color.red;
            }
            else
            {
                if (amountText != null) amountText.color = Color.green;
            }
        }

        if (repairButton != null) repairButton.interactable = canRepair;
    }

    public void OnRepairPressed()
    {
        if (currentStage >= stages.Count) return;

        var stage = stages[currentStage];

        // Double check itemů
        foreach (var req in stage.requirements)
        {
            if (inventoryManager.GetTotalItemCount(req.itemSO) < req.requiredAmount)
            {
                RefreshStageUI();
                return;
            }
        }

        // Odebrání itemů
        foreach (var req in stage.requirements)
            inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);

        // Odemčení tlačítek
        foreach (var btn in stage.worldButtonsToEnable)
        {
            if (btn != null) btn.interactable = true;
        }

        // Zvýšení stage
        currentStage++;

        // 🔥 Uložení hned po akci (pro jistotu, i když máme OnApplicationQuit)
        SaveCrystalData();

        RefreshStageUI();

        // Aktualizace vizuálu
        if (visualController != null)
        {
            visualController.UpdateVisuals(currentStage);
            visualController.PlayRepairEffect();
        }
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted.");
            currentStage = 0;
            RefreshStageUI();
            if (visualController != null) visualController.UpdateVisuals(0);
        }
    }
}