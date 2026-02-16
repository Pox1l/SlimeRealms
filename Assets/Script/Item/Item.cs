using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO itemData;
    public int quantity = 1;

    // Změněno na OnTriggerEnter2D a Collider2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (itemData == null) return;

        // 🔍 nejdřív zjistit, jestli je místo
        if (InventoryManager.Instance.IsInventoryFull(itemData, quantity))
        {
            if (PickupNotificationManager.Instance != null)
            {
                // čistá textová hláška
                PickupNotificationManager.Instance.ShowMessage("Inventory is full!");
            }
            return; // nic nebereme, item zůstává ležet
        }

        // normální přidání itemu
        int leftOver = InventoryManager.Instance.AddItem(itemData, quantity);
        int pickedAmount = quantity - leftOver;

        if (pickedAmount > 0 && PickupNotificationManager.Instance != null)
        {
            PickupNotificationManager.Instance.ShowPickup(
                itemData.icon,
                itemData.itemName,
                pickedAmount
            );
        }

        if (leftOver <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            quantity = leftOver;
        }
    }
}