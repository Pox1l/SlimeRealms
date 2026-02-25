using UnityEngine;
using FMODUnity; // Důležité pro práci s EventReference

public class Item : MonoBehaviour
{
    public ItemSO itemData;
    public int quantity = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kontrola, zda do itemu narazil hráč
        if (!collision.gameObject.CompareTag("Player")) return;
        if (itemData == null) return;

        // 1. Zjistit, jestli je místo v inventáři
        if (InventoryManager.Instance.IsInventoryFull(itemData, quantity))
        {
            if (PickupNotificationManager.Instance != null)
            {
                PickupNotificationManager.Instance.ShowMessage("Inventory is full!");
            }
            return; // Plno -> nic se nebere, zvuk nehraje
        }

        // 2. Přidání itemu do inventáře
        int leftOver = InventoryManager.Instance.AddItem(itemData, quantity);
        int pickedAmount = quantity - leftOver;

        // 3. POKUD SE NĚCO SEBRALO -> PŘEHRAJ ZVUK A UKAŽ NOTIFIKACI
        if (pickedAmount > 0)
        {
            // --- AUDIO LOGIKA ---
            if (AudioManager.instance != null)
            {
                // Voláme novou funkci v Manageru.
                // Ta automaticky zkontroluje:
                // Má item svůj zvuk? -> Hraj itemData.pickupSound
                // Je prázdný? -> Hraj defaultPickupSound z Manageru
                AudioManager.instance.PlayPickupSound(itemData.pickupSound);
            }
            // --------------------

            if (PickupNotificationManager.Instance != null)
            {
                PickupNotificationManager.Instance.ShowPickup(
                    itemData.icon,
                    itemData.itemName,
                    pickedAmount
                );
            }
        }

        // 4. Úklid objektu ze scény
        if (leftOver <= 0)
        {
            // Item se zničí, ale zvuk dohraje, protože ho spustil AudioManager
            Destroy(gameObject);
        }
        else
        {
            // Pokud se nevešlo všechno, zbytek zůstane ležet
            quantity = leftOver;
        }
    }
}