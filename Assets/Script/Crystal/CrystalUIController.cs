using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using FMODUnity; // 🔥 PŘIDÁNO: Knihovna pro FMOD

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
        public List<Button> worldButtonsToEnable = new List<Button>();
    }

    [System.Serializable]
    public class CrystalSaveData
    {
        public int savedStageIndex;
        public int totalStagesSaved;
    }

    [Header("References")]
    public InventoryManager inventoryManager;
    public CrystalVisualController visualController;
    public GameObject mainPanel;

    [Header("UI – požadavky")]
    public Transform requirementsParent;
    public GameObject requirementPrefab;
    public Button repairButton;

    [Header("Audio")] // 🔥 PŘIDÁNO: Sekce pro zvuk
    public EventReference spawnWorldSound;

    [Header("Stages")]
    public List<CrystalStage> stages = new List<CrystalStage>();

    private int currentStage = 0;
    private string saveFilePath;
    private int totalStages;

    void Awake()
    {
        saveFilePath = ProfileManager.GetSavePath("crystal_save.json");
        totalStages = stages.Count;
    }

    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        LoadCrystalData();
        LockAllWorldButtons();
        UnlockCompletedStages();

        if (visualController != null) visualController.UpdateVisuals(currentStage);

        // Aktualizace hned na začátku
        RefreshStageUI();

        // Automatická aktualizace, když se změní inventář
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += RefreshStageUI;
        }

        // --- 🔥 PŘIDÁNO: Přiřazení zvuku na všechna tlačítka světů ---
        foreach (var stage in stages)
        {
            foreach (var btn in stage.worldButtonsToEnable)
            {
                if (btn != null)
                {
                    btn.onClick.AddListener(PlaySpawnWorldSound);
                }
            }
        }
        // -------------------------------------------------------------
    }

    // 🔥 PŘIDÁNO: Metoda pro přehrání zvuku
    private void PlaySpawnWorldSound()
    {
        if (!spawnWorldSound.IsNull)
        {
            RuntimeManager.PlayOneShot(spawnWorldSound);
        }
    }

    // Důležité pro čištění paměti
    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshStageUI;
        }
    }

    // Metoda pro otevření UI - TADY SE DĚJE TO, CO JSI CHTĚL
    public void OpenUI()
    {
        mainPanel.SetActive(true);
        Time.timeScale = 0;

        // Tohle zajistí, že se zkontrolují itemy hned při otevření
        RefreshStageUI();
    }

    public void CloseUI()
    {
        mainPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void RefreshStageUI()
    {
        // Pojistka, pokud nejsou nastavené reference
        if (inventoryManager == null) return;

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

            // Získání počtu itemů z inventáře
            int owned = inventoryManager.GetTotalItemCount(req.itemSO);

            if (amountText != null)
                amountText.text = $"{owned} / {req.requiredAmount}"; // Upravil jsem text, aby ukazoval "Máš / Potřebuješ"

            // Kontrola, zda má hráč dostatek
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

        // Povolí nebo zakáže tlačítko podle toho, jestli má hráč všechno
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
        SaveCrystalData();
        RefreshStageUI();

        if (visualController != null)
        {
            visualController.UpdateVisuals(currentStage);
            visualController.PlayRepairEffect();
        }
    }

    public void SaveCrystalData()
    {
        CrystalSaveData data = new CrystalSaveData();
        data.savedStageIndex = currentStage;
        data.totalStagesSaved = stages.Count; //obrazení kdekoliv

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"💾 Crystal saved to {saveFilePath}");
    }

    public void LoadCrystalData()
    {
        if (!File.Exists(saveFilePath))
        {
            currentStage = 0;
            SaveCrystalData();
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            CrystalSaveData data = JsonUtility.FromJson<CrystalSaveData>(json);
            currentStage = data.savedStageIndex;
        }
        catch
        {
            currentStage = 0;
            SaveCrystalData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCrystalData();
    }

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

    public float GetRepairPercentage()
    {
        if (totalStages == 0) return 100f;
        float percentage = ((float)currentStage / totalStages) * 100f;
        return Mathf.Min(percentage, 100f);
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentStage = 0;
            RefreshStageUI();
            if (visualController != null) visualController.UpdateVisuals(0);
        }
    }

    public string GetProgressString()
    {
        // Vrátí například "1 / 4"
        return $"{currentStage} / {stages.Count}";
    }

    public int GetCurrentStageInt()
    {
        return currentStage;
    }
}