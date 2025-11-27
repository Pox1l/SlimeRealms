using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 1. Nutné pro detekci změny scény

public class PlayerUIManager : MonoBehaviour
{
    // 💡 Singleton
    public static PlayerUIManager Instance;

    [Header("UI - Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("UI - Energy")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("References (auto-assigned)")]
    private PlayerStats stats;
    private PlayerMovement movement;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            // 2. Přihlásíme se k odběru události načtení scény
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    // 3. Odhlásíme se, když objekt zanikne (prevence chyb)
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 4. Tato metoda se spustí AUTOMATICKY při každém načtení scény
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIElements();    // Najde nové slidery a texty podle TAGŮ
        FindAndSetupPlayer(); // Najde nového hráče
    }

    void Start()
    {
        // Pro jistotu voláme i na začátku (pro první scénu)
        FindUIElements();
        FindAndSetupPlayer();
    }

    // 🔍 Hledání UI prvků podle TAGU (Musíš vytvořit tyto tagy v Unity!)
    void FindUIElements()
    {
        // Health Slider
        GameObject hSliderObj = GameObject.FindGameObjectWithTag("HealthSlider");
        if (hSliderObj != null)
            healthSlider = hSliderObj.GetComponent<Slider>();
        else
            Debug.LogWarning("UI Manager: Nenašel jsem objekt s tagem 'HealthSlider'!");

        // Health Text
        GameObject hTextObj = GameObject.FindGameObjectWithTag("HealthText");
        if (hTextObj != null)
            healthText = hTextObj.GetComponent<TextMeshProUGUI>();

        // Energy Slider
        GameObject eSliderObj = GameObject.FindGameObjectWithTag("EnergySlider");
        if (eSliderObj != null)
            energySlider = eSliderObj.GetComponent<Slider>();

        // Energy Text
        GameObject eTextObj = GameObject.FindGameObjectWithTag("EnergyText");
        if (eTextObj != null)
            energyText = eTextObj.GetComponent<TextMeshProUGUI>();
    }

    // 🔍 Hledání Hráče
    void FindAndSetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetupPlayerReferences(player);
        }
        else
        {
            // Debug.Log("UI Manager: Hráč zatím není ve scéně.");
        }
    }

    public void SetupPlayerReferences(GameObject player)
    {
        stats = player.GetComponent<PlayerStats>();
        movement = player.GetComponent<PlayerMovement>();

        // Zaregistruj eventy a aktualizuj UI
        if (stats != null)
        {
            // Bezpečné přeregistrování (odstranit staré -> přidat nové)
            stats.OnHealthChanged -= UpdateHealth;
            stats.OnHealthChanged += UpdateHealth;
            UpdateHealth(stats.currentHealth, stats.maxHealth);
        }

        if (movement != null)
        {
            movement.OnEnergyChanged -= UpdateEnergy;
            movement.OnEnergyChanged += UpdateEnergy;
            UpdateEnergy(movement.currentEnergy, movement.maxEnergy);
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