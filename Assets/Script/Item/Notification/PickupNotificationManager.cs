using UnityEngine;

public class PickupNotificationManager : MonoBehaviour
{
    public static PickupNotificationManager Instance { get; private set; }

    [Header("UI")]
    public Transform container;
    public PickupNotificationEntry entryPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Klasické oznámení při sebrání itemu.
    /// </summary>
    public void ShowPickup(Sprite icon, string itemName, int amount)
    {
        if (entryPrefab == null || container == null)
        {
            Debug.LogWarning("PickupNotificationManager: chybí prefab nebo container!");
            return;
        }

        // ❌ UŽ NEKONTROLUJEME amount <= 0

        var entry = Instantiate(entryPrefab, container);
        entry.transform.SetAsFirstSibling();

        string msg = amount > 0 ? $"+{amount} {itemName}" : itemName;
        entry.Setup(icon, msg);
    }

    /// <summary>
    /// Obecná textová zpráva (chyba apod.).
    /// </summary>
    public void ShowMessage(string message)
    {
        if (entryPrefab == null || container == null)
        {
            Debug.LogWarning("PickupNotificationManager: chybí prefab nebo container!");
            return;
        }

        var entry = Instantiate(entryPrefab, container);
        entry.transform.SetAsFirstSibling();

        // žádná ikona, jen text
        entry.Setup(null, message);
    }
}
