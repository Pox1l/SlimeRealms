using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    // 💡 Singleton instance pro zajištění, že je jen jeden UIManager
    public static PlayerUIManager Instance;

    [Header("UI - Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("UI - Energy")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("References (auto-assigned)")]
    // Tyto reference už nemusí být veřejné/viditelné v Inspectoru, pokud je nastavujeme v kódu
    private PlayerStats stats;
    private PlayerMovement movement;

    void Awake()
    {
        // 1. Singleton: Kontrola duplicit
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            // 2. DontDestroyOnLoad: Uchování objektu mezi scénami
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // Pokud už instance existuje, tuto novou znič
            Destroy(this.gameObject);
            return; // Ukončíme metodu, aby se nespustil zbytek kódu
        }
    }

    void Start()
    {
        // ⚠️ Poznámka: Pokud se hráč nenačte hned na začátku scény, bude potřeba
        // najít ho později (např. v metodě, která se zavolá, když se hráč objeví).

        // Najdi objekt s tagem "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ PlayerUIManager: Žádný objekt s tagem 'Player' nebyl nalezen při startu! Pokusí se najít později.");
            // Nezastavíme se, protože UI manager má zůstat, ale nebudeme se pokoušet
            // nastavit reference, které nejsou dostupné.
            return;
        }

        // Získej reference na komponenty
        SetupPlayerReferences(player);
    }

    // 💡 Nová metoda pro nastavení referencí
    public void SetupPlayerReferences(GameObject player)
    {
        stats = player.GetComponent<PlayerStats>();
        movement = player.GetComponent<PlayerMovement>();

        if (stats == null)
            Debug.LogError("❌ PlayerUIManager: PlayerStats nebyl nalezen na objektu s tagem Player!");

        if (movement == null)
            Debug.LogError("❌ PlayerUIManager: PlayerMovement nebyl nalezen na objektu s tagem Player!");

        // Zaregistruj eventy a inicializuj UI
        if (stats != null)
        {
            // Odregistruj pro případ, že by se volalo opakovaně (např. při znovuvytvoření hráče)
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