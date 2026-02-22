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

    // 🔍 ÚPRAVA ZDE: Slučování sebraných předmětů
    public void ShowPickup(Sprite icon, string itemName, int amount)
    {
        if (entryPrefab == null || container == null) return;

        // Jméno bez čísla (bude sloužit jako klíč pro hledání)
        string baseItemName = itemName;

        // 1. Zkusíme najít, jestli už hláška pro tento předmět svítí
        foreach (var item in pool)
        {
            if (item.gameObject.activeSelf && item.baseMessage == baseItemName)
            {
                // Pokud ano, přidáme jí sebraný počet a resetujeme časovač
                item.AddPickupAmount(amount);
                item.transform.SetAsFirstSibling(); // Hodíme ji navrch seznamu
                return;
            }
        }

        // 2. Pokud se nenašla, vytvoříme novou
        var entry = GetFromPool();
        // Setup pro novou hlášku (pošleme počet a výchozí jméno)
        entry.SetupPickup(icon, baseItemName, amount);
    }

    public void ShowMessage(string message)
    {
        if (entryPrefab == null || container == null) return;

        foreach (var item in pool)
        {
            if (item.gameObject.activeSelf && item.baseMessage == message)
            {
                item.AddMessageCount(); // Zvýšíme číslo
                item.transform.SetAsFirstSibling();
                return;
            }
        }

        var entry = GetFromPool();
        entry.SetupMessage(inventoryFullIcon, message, true);
    }
}