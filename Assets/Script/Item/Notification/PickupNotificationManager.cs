using UnityEngine;
using System.Collections.Generic;

public class PickupNotificationManager : MonoBehaviour
{
    public static PickupNotificationManager Instance { get; private set; }

    [Header("UI")]
    public Transform container;
    public PickupNotificationEntry entryPrefab;

    [Header("Ikony")]
    public Sprite inventoryFullIcon;

    private List<PickupNotificationEntry> pool = new List<PickupNotificationEntry>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    } 

    private PickupNotificationEntry GetFromPool()
    {
        foreach (var item in pool)
        {
            if (!item.gameObject.activeSelf)
            {
                return item;
            }
        }

        var newEntry = Instantiate(entryPrefab, container);
        pool.Add(newEntry);
        return newEntry;
    }

    public void ShowPickup(Sprite icon, string itemName, int amount)
    {
        if (entryPrefab == null || container == null) return;

        var entry = GetFromPool();
        string msg = amount > 0 ? $"+{amount} {itemName}" : itemName;
        entry.Setup(icon, msg);
    }

    public void ShowMessage(string message)
    {
        if (entryPrefab == null || container == null) return;

        // 🔍 ÚPRAVA: Nejdřív zjistíme, jestli už stejná zpráva nesvítí na obrazovce
        foreach (var item in pool)
        {
            if (item.gameObject.activeSelf && item.baseMessage == message)
            {
                item.AddCount(true); // Zvýšíme číslo a prodloužíme čas
                item.transform.SetAsFirstSibling(); // Posuneme ji na začátek
                return;
            }
        }

        // Pokud neexistuje, vyrobíme novou
        var entry = GetFromPool();
        entry.Setup(inventoryFullIcon, message, true);
    }
}