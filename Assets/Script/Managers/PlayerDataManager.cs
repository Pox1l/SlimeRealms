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

            // ZMĚNA ZDE: Cestu si bereme přes ProfileManager
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
        // 🔥 POJISTKA: Pokud data neexistují (protože se smazal soubor), vytvoříme nová
        if (currentData == null)
        {
            currentData = new PlayerData();
        }

        currentData.currentHealth = curHP;
        currentData.maxHealth = maxHP;

        // Vždy uložíme max, aby v souboru nebylo divné číslo
        currentData.currentStamina = maxStamina;
        currentData.maxStamina = maxStamina;

        currentData.defense = defense;

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);

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
        currentData.currentHealth = -1; // -1 signalizuje PlayerStats, ať si HP dopočítá sám
        currentData.maxHealth = 100;
        currentData.currentStamina = 100;
        currentData.maxStamina = 100;
        currentData.defense = 25;

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);
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