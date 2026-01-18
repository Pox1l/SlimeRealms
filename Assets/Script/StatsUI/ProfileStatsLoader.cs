using UnityEngine;
using TMPro;
using System.IO; // Nutné pro práci se soubory

public class ProfileStatsLoader : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI crystalProgressText;

    private CrystalUIController crystalController;
    private string saveFilePath;

    void Awake()
    {
        // Musíme znát cestu k souboru (stejná jako v CrystalUIController)
        // Pøedpokládám, že ProfileManager je statický, pokud ne, použij Application.persistentDataPath
        saveFilePath = ProfileManager.GetSavePath("crystal_save.json");
    }

    void OnEnable()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        // 1. Zkusíme najít Controller ve scénì (pokud jsme v Hubu)
        GameObject crystalObj = GameObject.FindWithTag("KrystalCanvas");

        if (crystalObj != null)
        {
            // JSME V HUBU - Vezmeme data živì z controlleru
            crystalController = crystalObj.GetComponent<CrystalUIController>();
            if (crystalController != null)
            {
                crystalProgressText.text = "Crystal progression: " + crystalController.GetProgressString();
                return; // Hotovo, konèíme
            }
        }

        // 2. KRYSTAL NENÍ VE SCÉNÌ - Naèteme data ze souboru
        LoadFromFile();
    }

    void LoadFromFile()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                // Použijeme tøídu definovanou uvnitø CrystalUIController
                var data = JsonUtility.FromJson<CrystalUIController.CrystalSaveData>(json);

                // Pokud jsme ještì neuložili totalStagesSaved (starý save), dáme tam otazník nebo default
                int total = data.totalStagesSaved > 0 ? data.totalStagesSaved : 3;

                crystalProgressText.text = $"Crystal progression: {data.savedStageIndex} / {total}";
            }
            catch
            {
                crystalProgressText.text = "Crystal progression: Error";
            }
        }
        else
        {
            crystalProgressText.text = "Crystal progression: 0 / 0";
        }
    }
}