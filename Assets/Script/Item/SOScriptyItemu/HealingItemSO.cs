using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Healing Item")]
public class HealingItemSO : ItemSO
{
    [Header("Heal Settings")]
    public int healAmount = 20;

    public override bool UseItem()
    {
        // 1. POJISTKA PROTI CHYBĚ UNITY EDITORU
        if (lastTimeUsed > Time.time) lastTimeUsed = -999f;

        // 2. KONTROLA COOLDOWNU
        if (Time.time < lastTimeUsed + cooldown)
        {
            return false;
        }

        // 2. KONTROLA ZDRAVÍ
        if (PlayerStats.Instance != null)
        {
            // 🔥 Pokud máš plné životy, vrátíme false -> nic se nestane
            if (PlayerStats.Instance.currentHealth >= PlayerStats.Instance.maxHealth)
            {
                Debug.Log("Health is full!");
                return false;
            }

            // Pokud projdeme kontrolami, vyléčíme hráče
            PlayerStats.Instance.Heal(healAmount);

            // Nastavíme čas použití pro cooldown
            lastTimeUsed = Time.time;

            return true; // Item se spotřeboval
        }
        return false;
    }
}