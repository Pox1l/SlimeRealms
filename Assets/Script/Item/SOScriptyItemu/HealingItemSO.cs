using UnityEngine;
using FMODUnity; // 🔥 PŘIDÁNO: Knihovna pro FMOD

[CreateAssetMenu(menuName = "Inventory/Healing Item")]
public class HealingItemSO : ItemSO
{
    [Header("Heal Settings")]
    public int healAmount = 20;

    // UPRAVENO: Přidán parametr pro hlášku
    public override bool UseItem(out string failMessage)
    {
        failMessage = "";

        // 1. POJISTKA PROTI CHYBĚ UNITY EDITORU
        if (lastTimeUsed > Time.time) lastTimeUsed = -999f;

        // 2. KONTROLA COOLDOWNU
        if (Time.time < lastTimeUsed + cooldown)
        {
            failMessage = "Item is on cooldown!"; // UPRAVENO místo Debug.Log
            return false;
        }

        // 3. KONTROLA ZDRAVÍ
        if (PlayerStats.Instance != null)
        {
            // 🔥 Pokud máš plné životy, vrátíme false -> nic se nestane
            if (PlayerStats.Instance.currentHealth >= PlayerStats.Instance.maxHealth)
            {
                failMessage = "HP is full!"; // UPRAVENO místo Debug.Log
                return false;
            }

            // Pokud projdeme kontrolami, vyléčíme hráče
            PlayerStats.Instance.Heal(healAmount);

            // Nastavíme čas použití pro cooldown
            lastTimeUsed = Time.time;

            // --- 🔥 PŘIDÁNO: Přehrání zvuku při úspěšném použití ---
            if (!useSound.IsNull)
            {
                RuntimeManager.PlayOneShot(useSound);
            }
            // -------------------------------------------------------

            return true; // Item se spotřeboval
        }

        failMessage = "Chyba hráče!"; // Pokud nenajde PlayerStats
        return false;
    }
}