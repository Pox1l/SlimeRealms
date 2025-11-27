using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Třídy pro data necháme stejné
[System.Serializable]
public class InventorySlotData { public int itemID; public int quantity; }

[System.Serializable]
public class InventorySaveData { public List<InventorySlotData> slots = new List<InventorySlotData>(); }

public class InventorySaveSystem : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
    }

    private void Start()
    {
        // Load se volá už v InventoryManager.OnSceneLoaded, ale pro jistotu při startu
        LoadInventory(); 
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

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Inventory saved.");
    }

    public void LoadInventory()
    {
        if (ItemDatabase.Instance == null) return;
        if (!File.Exists(savePath)) return;
        if (InventoryManager.Instance == null || InventoryManager.Instance.itemSlots == null) return;

        string json = File.ReadAllText(savePath);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        var slots = InventoryManager.Instance.itemSlots;

        // Projdeme sloty a naplníme je daty
        for (int i = 0; i < slots.Length && i < data.slots.Count; i++)
        {
            if (slots[i] == null) continue; // Pojistka

            var slotData = data.slots[i];
            if (slotData.itemID >= 0)
            {
                ItemSO item = ItemDatabase.Instance.GetItemByID(slotData.itemID);
                if (item != null)
                {
                    slots[i].itemData = item;
                    slots[i].quantity = slotData.quantity;
                    
                    // Aktualizace vizuálu slotu
                    slots[i].SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                slots[i].itemData = null;
                slots[i].quantity = 0;
                slots[i].SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
            }
        }
        Debug.Log("📦 Inventory loaded into new slots!");
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}