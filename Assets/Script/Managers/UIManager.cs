using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI - Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("UI - Energy")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("Reference")]
    public PlayerStats stats;
    public PlayerMovement movement;

    void Start()
    {
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
        if (healthText != null) healthText.text = current + " / " + max;
    }

    void UpdateEnergy(float current, float max)
    {
        if (energySlider != null) energySlider.value = current / max;
        if (energyText != null) energyText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }
}
