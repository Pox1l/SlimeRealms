using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    [Header("UI Elementy")]
    public Slider healthSlider;     // Sem přetáhni objekt 'BossSlider'
    public TextMeshProUGUI hpText;  // Sem přetáhni 'BossHPText'

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 🔒 Na začátku hry UI schováme
        HideUI();
    }

    public void ShowUI()
    {
        // 1. Zapneme hlavní rodičovský objekt (BossUI)
        gameObject.SetActive(true);

        // 2. 🔥 VYNUTÍME zapnutí slideru a textu (child objektů)
        if (healthSlider != null) healthSlider.gameObject.SetActive(true);
        if (hpText != null) hpText.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        // Vypneme hlavní objekt (tím zmizí i děti)
        gameObject.SetActive(false);
    }

    public void UpdateHealth(int current, int max)
    {
        // Posuvník
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        // Text (např. "450 / 500")
        if (hpText != null)
        {
            hpText.text = $"{current} / {max}";
        }
    }
}