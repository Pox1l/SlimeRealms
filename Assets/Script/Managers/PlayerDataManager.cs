using UnityEngine;
using System.IO;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    [Header("Player Data")]
    public PlayerData currentData;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            savePath = ProfileManager.GetSavePath("player_save.json");

            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerStats(int curHP, int maxHP, float curStamina, float maxStamina, float defense)
    {
        if (currentData == null)
        {
            currentData = new PlayerData();
        }

        currentData.currentHealth = curHP;
        currentData.maxHealth = maxHP;
        currentData.currentStamina = maxStamina;
        currentData.maxStamina = maxStamina;
        currentData.defense = defense;

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);

        // PŘIDÁNO: Řekneme ikoně, ať blikne
        SaveVisual.ReportSave();

        Debug.Log($"💾 Player Data uložena do: {savePath}");
    }

    public void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentData = JsonUtility.FromJson<PlayerData>(json);
            }
            catch
            {
                Debug.LogWarning("Save file corrupted, creating new.");
                ResetData();
            }
        }
        else
        {
            ResetData();
        }
    }

    public void ResetData()
    {
        currentData = new PlayerData();
        currentData.currentHealth = -1;
        currentData.maxHealth = 100;
        currentData.currentStamina = 100;
        currentData.maxStamina = 100;
        currentData.defense = 25;

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);

        // PŘIDÁNO: I při resetu blikneme
        SaveVisual.ReportSave();
    }
}

[System.Serializable]
public class PlayerData
{
    public int currentHealth;
    public int maxHealth;
    public float currentStamina;
    public float maxStamina;
    public float defense;
}