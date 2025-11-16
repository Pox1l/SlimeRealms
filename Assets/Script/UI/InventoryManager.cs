using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slots")]
    public ItemSlot[] itemSlots;

    [Header("Description UI")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private bool menuActivated = false;
    public GameObject inventoryUI;

    private void Awake()
    {
        Instance = this;
    }

   
    public void ToggleInventory()
    {
        menuActivated = !menuActivated;
        inventoryUI.SetActive(menuActivated);
        Time.timeScale = menuActivated ? 0 : 1;
    }

    public void OpenInventory()
    {
        menuActivated = true;
        inventoryUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseInventory()
    {
        menuActivated = false;
        inventoryUI.SetActive(false);
        Time.timeScale = 1;
    }

    public int AddItem(ItemSO itemData, int quantity)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].itemData == null || itemSlots[i].itemData == itemData)
            {
                int leftOver = itemSlots[i].AddItem(itemData, quantity);
                if (leftOver > 0)
                {
                    quantity = leftOver;
                }
                else
                {
                    return 0;
                }
            }
        }
        return quantity;
    }

    public void ShowItemDescription(ItemSO item)
    {
        if (item == null)
        {
            descriptionIcon.enabled = false;
            descriptionName.text = "";
            descriptionText.text = "";
            return;
        }

        descriptionIcon.enabled = true;
        descriptionIcon.sprite = item.icon;
        descriptionName.text = item.itemName;
        descriptionText.text = item.description;
    }

    public void DeselectAllSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot.selectedShader.SetActive(false);
        }
    }

    public int GetTotalItemCount(ItemSO item)
    {
        int count = 0;
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item)
                count += slot.quantity;
        }
        return count;
    }

    public void RemoveItem(ItemSO item, int amount)
    {
        Debug.Log($"Inventory: RemoveItem {amount}x {item.name}");

        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item)
            {
                int remove = Mathf.Min(slot.quantity, amount);
                Debug.Log($"Inventory: beru {remove} ze slotu s {slot.quantity}");
                slot.RemoveItem(remove);
                amount -= remove;
                if (amount <= 0)
                {
                    Debug.Log("Inventory: hotovo, nic nezůstalo k odebrání");
                    return;
                }
            }
        }

        if (amount > 0)
        {
            Debug.LogWarning($"Inventory: nepodařilo se odebrat všechno, zbylo: {amount}");
        }
    }

    public bool IsInventoryFull(ItemSO item, int amount)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item && slot.quantity < item.maxStack)
            {
                return false;
            }
        }

        foreach (var slot in itemSlots)
        {
            if (slot.itemData == null)
                return false;
        }

        return true;
    }
}
