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

    [Header("Description UI")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private bool menuActivated = false;
    public InventorySaveSystem saveSystem; // Zpět na private

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
        }

        // Zkusíme najít komponentu na sobě
        saveSystem = GetComponent<InventorySaveSystem>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindUIReferences();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferences();

        // Pokud nemáme referenci, zkusíme ji najít přes TAG
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
        if (inventoryUI == null && CentralMenuUI.Instance != null)
        {
            inventoryUI = CentralMenuUI.Instance.inventoryPanel;
        }

        if (inventoryUI == null)
        {
            GameObject panel = GameObject.FindGameObjectWithTag("InventoryPanel");
            if (panel != null) inventoryUI = panel;
        }

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

        // Pokud nemáme referenci, zkusíme najít přes TAG (pojistka)
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

        // Pojistka pro saveSystem
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
}