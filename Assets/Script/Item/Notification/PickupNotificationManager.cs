using UnityEngine;

public class PickupNotificationManager : MonoBehaviour
{
    public static PickupNotificationManager Instance { get; private set; }

    [Header("UI")]
    public Transform container;                    // PickupNotificationPanel
    public PickupNotificationEntry entryPrefab;    // prefab oznámení

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
    /// Zavolej pøi sebrání itemu.
    /// </summary>
    public void ShowPickup(Sprite icon, string itemName, int amount)
    {
        if (entryPrefab == null || container == null)
        {
            Debug.LogWarning("PickupNotificationManager: chybí prefab nebo container!");
            return;
        }

        if (amount <= 0) return;

        PickupNotificationEntry entry = Instantiate(entryPrefab, container);
        entry.transform.SetAsFirstSibling();

        string msg = $"+{amount} {itemName}";
        entry.Setup(icon, msg);
    }
}
