using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{
    // 💡 Singleton
    public static PlayerUIManager Instance;

    [Header("UI - Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("UI - Stamina")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("References (auto-assigned)")]
    private PlayerStats stats;
    // private PlayerMovement movement; // ❌ Už nepotřebujeme, stamina je ve stats

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIElements();
        FindAndSetupPlayer();
    }

    void Start()
    {
        FindUIElements();
        FindAndSetupPlayer();
    }

    // 🔍 Hledání UI prvků podle TAGU
    void FindUIElements()
    {
        // Health Slider
        GameObject hSliderObj = GameObject.FindGameObjectWithTag("HealthSlider");
        if (hSliderObj != null) healthSlider = hSliderObj.GetComponent<Slider>();

        // Health Text
        GameObject hTextObj = GameObject.FindGameObjectWithTag("HealthText");
        if (hTextObj != null) healthText = hTextObj.GetComponent<TextMeshProUGUI>();

        // Energy Slider
        GameObject eSliderObj = GameObject.FindGameObjectWithTag("EnergySlider");
        if (eSliderObj != null) energySlider = eSliderObj.GetComponent<Slider>();

        // Energy Text
        GameObject eTextObj = GameObject.FindGameObjectWithTag("EnergyText");
        if (eTextObj != null) energyText = eTextObj.GetComponent<TextMeshProUGUI>();
    }

    // 🔍 Hledání Hráče
    void FindAndSetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetupPlayerReferences(player);
        }
    }

    public void SetupPlayerReferences(GameObject player)
    {
        stats = player.GetComponent<PlayerStats>();
        // movement = player.GetComponent<PlayerMovement>(); // Už nepotřebujeme pro UI

        // Zaregistruj eventy a aktualizuj UI
        if (stats != null)
        {
            // --- HEALTH ---
            stats.OnHealthChanged -= UpdateHealth; // Odhlásit staré (prevence bugů)
            stats.OnHealthChanged += UpdateHealth; // Přihlásit nové
            UpdateHealth(stats.currentHealth, stats.maxHealth);

            // --- STAMINA (Nové umístění) ---
            stats.OnStaminaChanged -= UpdateEnergy;
            stats.OnStaminaChanged += UpdateEnergy;
            UpdateEnergy(stats.currentStamina, stats.maxStamina);
        }
    }

    void UpdateHealth(int current, int max)
    {
        if (healthSlider != null) healthSlider.value = (float)current / max;
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    void UpdateEnergy(float current, float max)
    {
        if (energySlider != null) energySlider.value = current / max;
        if (energyText != null) energyText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }
}