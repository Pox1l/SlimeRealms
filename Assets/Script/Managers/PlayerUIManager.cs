using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI - Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("UI - Energy")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("References (auto-assigned)")]
    public PlayerStats stats;
    public PlayerMovement movement;

    void Start()
    {
        //  Najdi objekt s tagem "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ PlayerUIManager: Žádný objekt s tagem 'Player' nebyl nalezen!");
            return;
        }

        //  Získej reference na komponenty
        stats = player.GetComponent<PlayerStats>();
        movement = player.GetComponent<PlayerMovement>();

        if (stats == null)
            Debug.LogError("❌ PlayerUIManager: PlayerStats nebyl nalezen na objektu s tagem Player!");

        if (movement == null)
            Debug.LogError("❌ PlayerUIManager: PlayerMovement nebyl nalezen na objektu s tagem Player!");

        //  Zaregistruj eventy a inicializuj UI
        if (stats != null)
        {
            stats.OnHealthChanged += UpdateHealth;
            UpdateHealth(stats.currentHealth, stats.maxHealth);
        }

        if (movement != null)
        {
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
