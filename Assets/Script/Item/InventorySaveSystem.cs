using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InventorySlotData { public int itemID; public int quantity; }

[System.Serializable]
public class InventorySaveData
{
    public List<InventorySlotData> slots = new List<InventorySlotData>();
    public int quickSlotItemID = -1; // 🔥 PŘIDÁNO: Proměnná pro Quick Slot
}

public class InventorySaveSystem : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = ProfileManager.GetSavePath("inventory_save.json");
    }

    private void Start()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.itemSlots != null)
        {
            LoadInventory();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) SaveInventory();
        if (Input.GetKeyDown(KeyCode.F9)) LoadInventory();
    }

    public void SaveInventory()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.itemSlots == null) return;

        InventorySaveData data = new InventorySaveData();

        foreach (var slot in InventoryManager.Instance.itemSlots)
        {
            if (slot != null && slot.itemData != null)
            {
                data.slots.Add(new InventorySlotData { itemID = slot.itemData.itemID, quantity = slot.quantity });
            }
            else
            {
                data.slots.Add(new InventorySlotData { itemID = -1, quantity = 0 });
            }
        }

        // --- 🔥 PŘIDÁNO: Uložení Quick Slotu ---
        if (QuickSlotManager.Instance != null && QuickSlotManager.Instance.GetCurrentItem() != null)
        {
            data.quickSlotItemID = QuickSlotManager.Instance.GetCurrentItem().itemID;
        }
        else
        {
            data.quickSlotItemID = -1;
        }
        // ---------------------------------------

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Inventory saved to: {savePath}");
    }

    public void LoadInventory()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("⚠️ Cannot load inventory: ItemDatabase is missing!");
            return;
        }

        if (!File.Exists(savePath))
        {
            Debug.Log("ℹ️ No save file found.");
            return;
        }

        if (InventoryManager.Instance == null || InventoryManager.Instance.itemSlots == null) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        var slots = InventoryManager.Instance.itemSlots;

        for (int i = 0; i < slots.Length && i < data.slots.Count; i++)
        {
            if (slots[i] == null) continue;

            var slotData = data.slots[i];

            if (slotData.itemID >= 0)
            {
                ItemSO item = ItemDatabase.Instance.GetItemByID(slotData.itemID);
                if (item != null)
                {
                    slots[i].itemData = item;
                    slots[i].quantity = slotData.quantity;
                }
                else
                {
                    slots[i].itemData = null;
                    slots[i].quantity = 0;
                }
            }
            else
            {
                slots[i].itemData = null;
                slots[i].quantity = 0;
            }

            slots[i].UpdateUI();
        }

        // --- 🔥 PŘIDÁNO: Načtení Quick Slotu ---
        if (QuickSlotManager.Instance != null)
        {
            if (data.quickSlotItemID >= 0)
            {
                ItemSO quickItem = ItemDatabase.Instance.GetItemByID(data.quickSlotItemID);
                if (quickItem != null)
                {
                    QuickSlotManager.Instance.AssignItemToSlot(quickItem);
                }
            }
            else
            {
                QuickSlotManager.Instance.ClearSlot(); // Ujistí se, že je prázdný, pokud v savu nic nebylo
            }
        }
        // ---------------------------------------

        Debug.Log("📦 Inventory loaded!");
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}