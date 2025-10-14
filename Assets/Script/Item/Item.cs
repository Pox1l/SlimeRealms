using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO itemData;
    public int quantity = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            int leftOver = InventoryManager.Instance.AddItem(itemData, quantity);
            if (leftOver <= 0) Destroy(gameObject);
            else quantity = leftOver;
        }
    }
}
