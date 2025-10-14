using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private string savePath;

    void Awake()
    {
        // Cesta k JSON souboru
        savePath = Application.persistentDataPath + "/playerData.json";
        LoadHealth();
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
        SaveHealth();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
        SaveHealth();
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = (float)currentHealth / maxHealth;
        if (healthText != null) healthText.text = currentHealth + " / " + maxHealth;
    }

    [System.Serializable]
    class SaveData
    {
        public int health;
    }

    public void SaveHealth()
    {
        SaveData data = new SaveData();
        data.health = currentHealth;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
    }

    public void LoadHealth()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            currentHealth = data.health;
        }
        else
        {
            currentHealth = maxHealth; // první start
        }
    }
}
