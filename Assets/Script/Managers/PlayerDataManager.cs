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
        // --- SINGLETON PATTERN (DONT DESTROY) ---
        if (Instance == null)
        {
            // Pokud jsem první manager ve høe, nastavím se jako HLAVNÍ
            Instance = this;

            // Tohle zajistí, že tento objekt nezmizí pøi naètení nové scény
            DontDestroyOnLoad(gameObject);

            // Nastavení cesty a naètení dat
            savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            LoadPlayerData();
        }
        else
        {
            // Pokud už jeden Manager existuje (pøišel z minulé scény),
            // tak tento NOVÝ se musí znièit, jinak by byly dva.
            Destroy(gameObject);
        }
    }

    // --- ZBYTEK LOGIKY ---

    public void SavePlayerData(int currentHealth)
    {
        currentData.currentHealth = currentHealth;
        // currentData.currentStamina = ... (zde mùžeš pøidat další)

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);
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
        currentData.currentHealth = -1; // -1 = Plné zdraví pøi startu
        currentData.currentStamina = 100;
        SavePlayerData(-1);
    }
}

[System.Serializable]
public class PlayerData
{
    public int currentHealth;
    public float currentStamina;
}