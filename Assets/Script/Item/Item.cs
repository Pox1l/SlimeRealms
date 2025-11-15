using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO itemData;
    public int quantity = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (itemData == null) return;

        // Pøidání do inventáøe
        int leftOver = InventoryManager.Instance.AddItem(itemData, quantity);

        // Kolik se reálnì sebralo
        int pickedAmount = quantity - leftOver;

        // Oznámení – jen když jsme nìco fakt sebrali
        if (pickedAmount > 0 && PickupNotificationManager.Instance != null)
        {
            PickupNotificationManager.Instance.ShowPickup(
                itemData.icon,
                itemData.itemName,
                pickedAmount
            );
        }

        // Logika pro zbytek na zemi
        if (leftOver <= 0)
        {
            // všechno se vešlo -> item zmizí
            Destroy(gameObject);
        }
        else
        {
            // nìco zùstalo na zemi – quantity se aktualizuje
            quantity = leftOver;
        }
    }
}
