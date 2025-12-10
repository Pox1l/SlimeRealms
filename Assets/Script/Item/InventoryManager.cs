using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public GameObject inventoryUI;
    public ItemSlot[] itemSlots;

    [Header("Context Menu")]
    public InventoryContextMenu contextMenu;

    [Header("Description UI")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private bool menuActivated = false;
    public InventorySaveSystem saveSystem;

    private void Awake()
    {
        // Singleton logika...
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

        // Zkusíme najít komponentu na sobě
        saveSystem = GetComponent<InventorySaveSystem>();

        // Hned najdeme reference
        FindUIReferences();
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
            {
                saveSystem.LoadInventory();
            }
            else
            {
                Debug.LogWarning("⚠️ ItemDatabase není připravena.");
            }
        }
    }

    void FindUIReferences()
    {
        // 1. Najdeme hlavní panel inventáře
        if (inventoryUI == null && CentralMenuUI.Instance != null)
        {
            inventoryUI = CentralMenuUI.Instance.inventoryPanel;
        }

        if (inventoryUI == null)
        {
            GameObject panel = GameObject.FindGameObjectWithTag("InventoryPanel");
            if (panel != null) inventoryUI = panel;
        }

        // 2. Pokud máme panel, prohledáme jeho děti
        if (inventoryUI != null)
        {
            Transform[] allChildren = inventoryUI.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allChildren)
            {
                if (t.CompareTag("InventorySlotsParent"))
                {
                    itemSlots = t.GetComponentsInChildren<ItemSlot>(true);
                }
                else if (t.CompareTag("InvDescIcon"))
                {
                    descriptionIcon = t.GetComponent<Image>();
                }
                else if (t.CompareTag("InvDescName"))
                {
                    descriptionName = t.GetComponent<TMP_Text>();
                }
                else if (t.CompareTag("InvDescText"))
                {
                    descriptionText = t.GetComponent<TMP_Text>();
                }
            }
        }

        // --- HLEDÁNÍ KONTEXTOVÉHO MENU (OPRAVENO) ---

        // Protože jsme z ContextMenu udělali Singleton, stačí vzít jeho Instance.
        // Je to spolehlivější než hledání přes Tagy.
        if (contextMenu == null)
        {
            contextMenu = InventoryContextMenu.Instance;
        }

        // Pojistka: Kdyby Instance ještě nebyla (vzácné), zkusíme najít typem
        if (contextMenu == null)
        {
            contextMenu = FindObjectOfType<InventoryContextMenu>(true);
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

        int originalQuantity = quantity;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].itemData == null || itemSlots[i].itemData == itemData)
            {
                int leftOver = itemSlots[i].AddItem(itemData, quantity);

                if (leftOver < quantity)
                {
                    quantity = leftOver;
                    if (saveSystem != null) saveSystem.SaveInventory();
                }

                if (quantity <= 0) return 0;
            }
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

        if (itemRemoved && saveSystem != null)
        {
            saveSystem.SaveInventory();
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
        // 1. Priorita Hover: Pokud je hoveredItem platný (myš je nad něčím), zobraz ho
        if (hoveredItem != null)
        {
            ShowItemDescription(hoveredItem);
            return;
        }

        // 2. Záložní Popis: Pokud myš není nad ničím (hoveredItem == null), 
        // zkusíme ukázat popis vybraného slotu.
        if (ItemSlot.currentSelectedSlot != null && ItemSlot.currentSelectedSlot.itemData != null)
        {
            ShowItemDescription(ItemSlot.currentSelectedSlot.itemData);
        }
        else
        {
            // 3. Žádný hover a žádný vybraný item: Vynulujeme popis
            ShowItemDescription(null);
        }
    }
}