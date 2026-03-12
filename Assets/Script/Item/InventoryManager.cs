using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System; // 🔥 DŮLEŽITÉ: Přidáno pro Action

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // 🔥 TOTO JE TA NOVINKA - Událost, kterou budou ostatní poslouchat
    public event Action OnInventoryChanged;

    [Header("UI References")]
    public GameObject inventoryUI;
    public ItemSlot[] itemSlots;
    public InventorySlotHolder slotHolder;
    public InventoryDescriptionHolder descriptionHolder;

    [Header("Context Menu")]
    public InventoryContextMenu contextMenu;

    [Header("Description UI (Auto-filled via Holder)")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    // 🔥 PŘIDÁNO: Right Click Ikonka
    [Header("Right Click Hint")]
    public GameObject rightClickIcon;
    public Vector3 rightClickOffset = new Vector3(50, -50, 0);

    private bool menuActivated = false;
    public InventorySaveSystem saveSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        saveSystem = GetComponent<InventorySaveSystem>();
        FindUIReferences();

        // 🔥 PŘIDÁNO: Skrytí ikonky na začátku
        ShowRightClickHint(false);
    }

    // 🔥 PŘIDÁNO: Update pro pohyb ikonky za myší
    private void Update()
    {
        if (rightClickIcon != null && rightClickIcon.activeSelf)
        {
            rightClickIcon.transform.position = Input.mousePosition + rightClickOffset;
        }
    }

    // 🔥 PŘIDÁNO: Metoda pro zobrazení/skrytí ikonky
    public void ShowRightClickHint(bool show)
    {
        if (rightClickIcon != null)
        {
            rightClickIcon.SetActive(show);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferences();

        if (saveSystem == null)
        {
            GameObject saveObj = GameObject.FindGameObjectWithTag("itemSaveSys");
            if (saveObj != null) saveSystem = saveObj.GetComponent<InventorySaveSystem>();
        }

        if (saveSystem != null && itemSlots != null && itemSlots.Length > 0)
        {
            if (ItemDatabase.Instance != null)
                saveSystem.LoadInventory();
            else
                Debug.LogWarning("⚠️ ItemDatabase není připravena.");
        }
    }

    void FindUIReferences()
    {
        // 1. Najdeme hlavní panel inventáře
        if (inventoryUI == null && CentralMenuUI.Instance != null)
            inventoryUI = CentralMenuUI.Instance.inventoryPanel;

        if (inventoryUI == null)
        {
            GameObject panel = GameObject.FindGameObjectWithTag("InventoryPanel");
            if (panel != null) inventoryUI = panel;
        }

        // 2. Hledání SLOTŮ přes Holder
        if (slotHolder == null)
            slotHolder = FindObjectOfType<InventorySlotHolder>(true);

        if (slotHolder != null)
            itemSlots = slotHolder.GetComponentsInChildren<ItemSlot>(true);

        // 3. Hledání POPISU přes Holder
        if (descriptionHolder == null)
            descriptionHolder = FindObjectOfType<InventoryDescriptionHolder>(true);

        if (descriptionHolder != null)
        {
            descriptionIcon = descriptionHolder.icon;
            descriptionName = descriptionHolder.itemName;
            descriptionText = descriptionHolder.itemDescription;
        }

        // 4. Záložní logika
        if (inventoryUI != null && (slotHolder == null || descriptionHolder == null))
        {
            Transform[] allChildren = inventoryUI.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allChildren)
            {
                if (slotHolder == null && t.CompareTag("InventorySlotsParent"))
                {
                    itemSlots = t.GetComponentsInChildren<ItemSlot>(true);
                }

                if (descriptionHolder == null)
                {
                    if (t.CompareTag("InvDescIcon")) descriptionIcon = t.GetComponent<Image>();
                    else if (t.CompareTag("InvDescName")) descriptionName = t.GetComponent<TMP_Text>();
                    else if (t.CompareTag("InvDescText")) descriptionText = t.GetComponent<TMP_Text>();
                }
            }
        }

        // 5. Kontextové menu
        if (contextMenu == null) contextMenu = InventoryContextMenu.Instance;
        if (contextMenu == null) contextMenu = FindObjectOfType<InventoryContextMenu>(true);

        // 6. 🔥 PŘIDÁNO: Hledání Right Click Ikonky (Opraveno pro DontDestroyOnLoad)
        if (rightClickIcon == null || !rightClickIcon) // Detekuje i "zničené" Unity objekty
        {
            rightClickIcon = null; // Vyčistíme starou mrtvou referenci

            // Bezpečnější hledání i mezi vypnutými objekty
            Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas c in allCanvases)
            {
                if (c.name == "CentralMenuCanvas")
                {
                    Transform[] children = c.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in children)
                    {
                        if (t.name == "rightClickSprite" || t.CompareTag("rightClickIC"))
                        {
                            rightClickIcon = t.gameObject;
                            break; // Našli jsme, ukončíme cyklus
                        }
                    }
                }
            }
        }
    }

    public void ToggleInventory()
    {
        menuActivated = !menuActivated;
        if (inventoryUI) inventoryUI.SetActive(menuActivated);
        Time.timeScale = menuActivated ? 0 : 1;
    }

    public void OpenInventory()
    {
        menuActivated = true;
        if (inventoryUI) inventoryUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseInventory()
    {
        menuActivated = false;
        if (inventoryUI) inventoryUI.SetActive(false);
        Time.timeScale = 1;
    }

    public int AddItem(ItemSO itemData, int quantity)
    {
        if (itemSlots == null) return quantity;

        if (saveSystem == null)
        {
            GameObject saveObj = GameObject.FindGameObjectWithTag("itemSaveSys");
            if (saveObj != null) saveSystem = saveObj.GetComponent<InventorySaveSystem>();
        }

        bool itemAdded = false;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].itemData == null || itemSlots[i].itemData == itemData)
            {
                int leftOver = itemSlots[i].AddItem(itemData, quantity);

                if (leftOver < quantity)
                {
                    quantity = leftOver;
                    itemAdded = true;
                }

                if (quantity <= 0) break;
            }
        }

        if (itemAdded)
        {
            if (saveSystem != null) saveSystem.SaveInventory();
            OnInventoryChanged?.Invoke();
            return 0;
        }

        return quantity;
    }

    public void RemoveItem(ItemSO item, int amount)
    {
        if (itemSlots == null) return;

        if (saveSystem == null)
        {
            GameObject saveObj = GameObject.FindGameObjectWithTag("itemSaveSys");
            if (saveObj != null) saveSystem = saveObj.GetComponent<InventorySaveSystem>();
        }

        bool itemRemoved = false;
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item)
            {
                int remove = Mathf.Min(slot.quantity, amount);
                slot.RemoveItem(remove);
                amount -= remove;
                itemRemoved = true;
                if (amount <= 0) break;
            }
        }

        if (itemRemoved)
        {
            if (saveSystem != null) saveSystem.SaveInventory();
            OnInventoryChanged?.Invoke();
        }
    }

    public void ShowItemDescription(ItemSO item)
    {
        if (descriptionIcon == null || descriptionName == null || descriptionText == null) return;

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
        if (itemSlots == null) return;
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.selectedShader != null)
                slot.selectedShader.SetActive(false);
        }
    }

    public int GetTotalItemCount(ItemSO item)
    {
        int count = 0;
        if (itemSlots == null) return 0;
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.itemData == item)
                count += slot.quantity;
        }
        return count;
    }

    public bool IsInventoryFull(ItemSO item, int amount)
    {
        if (itemSlots == null) return true;
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item && slot.quantity < item.maxStack) return false;
        }
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == null) return false;
        }
        return true;
    }

    public void TryShowSelectedDescription(ItemSO hoveredItem)
    {
        if (hoveredItem != null)
        {
            ShowItemDescription(hoveredItem);
            return;
        }

        if (ItemSlot.currentSelectedSlot != null && ItemSlot.currentSelectedSlot.itemData != null)
        {
            ShowItemDescription(ItemSlot.currentSelectedSlot.itemData);
        }
        else
        {
            ShowItemDescription(null);
        }
    }
}