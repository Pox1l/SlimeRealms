using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI Elementy")]
    public GameObject rootPanel;    // Hlavní objekt, který se bude vypínat/zapínat
    public Slider healthSlider;     // Slider života
    public TextMeshProUGUI hpText;  // Text "500 / 500"
    public TextMeshProUGUI nameText; // Jméno bosse (volitelné)

    // Inicializace hodnot (volá UIManager na začátku boje)
    public void Init(string bossName, int maxHP)
    {
        if (nameText != null) nameText.text = bossName;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = maxHP;
        }
        UpdateHealth(maxHP, maxHP);
    }

    // Aktualizace vizuálu (volá UIManager při zásahu)
    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = current;
        }

        if (hpText != null)
        {
            hpText.text = $"{current} / {max}";
        }
    }

    
    public void ToggleVisibility(bool state)
    {
        if (rootPanel != null)
            rootPanel.SetActive(state);
        else
            gameObject.SetActive(state); 
    }
}