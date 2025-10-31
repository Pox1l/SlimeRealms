using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventorySlotData> slots = new List<InventorySlotData>();
}

public class InventorySaveSystem : MonoBehaviour
{
    private string savePath;

    private void Start()
    {
        LoadInventory();  // ✅ automaticky načte při spuštění hry
    }

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveInventory(); // 💾 ruční save
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadInventory(); // 📦 ruční load
        }
    }

    // ✅ Uloží inventář do JSON souboru
    public void SaveInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager not found!");
            return;
        }

        InventorySaveData data = new InventorySaveData();

        foreach (var slot in InventoryManager.Instance.itemSlots)
        {
            if (slot.itemData != null)
            {
                InventorySlotData slotData = new InventorySlotData
                {
                    itemID = slot.itemData.itemID,
                    quantity = slot.quantity
                };
                data.slots.Add(slotData);
            }
            else
            {
                // prázdný slot se ukládá jako null slot
                data.slots.Add(new InventorySlotData { itemID = -1, quantity = 0 });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"💾 Inventory saved to {savePath}");
    }

    // ✅ Načte inventář ze souboru
    public void LoadInventory()
    {

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("❌ ItemDatabase.Instance is NULL! Ujisti se, že je ve scéně.");
            return;
        }

        if (!File.Exists(savePath))
        {
            Debug.Log("No inventory save found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager not found!");
            return;
        }

        var slots = InventoryManager.Instance.itemSlots;
        for (int i = 0; i < slots.Length && i < data.slots.Count; i++)
        {
            var slotData = data.slots[i];
            if (slotData.itemID >= 0)
            {
                ItemSO item = ItemDatabase.Instance.GetItemByID(slotData.itemID);
                if (item != null)
                {
                    slots[i].itemData = item;
                    slots[i].quantity = slotData.quantity;
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

        Debug.Log("📦 Inventory loaded!");
    }

    // ✅ Automaticky uloží při vypnutí hry
    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}
