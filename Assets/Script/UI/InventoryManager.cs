using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public GameObject inventoryUI; // Celý panel inventáře
    public ItemSlot[] itemSlots;   // Pole slotů

    [Header("Description UI")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private bool menuActivated = false;
    private InventorySaveSystem saveSystem;

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
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        saveSystem = GetComponent<InventorySaveSystem>();
        // Zkusíme najít UI hned na začátku
        FindUIReferences();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferences();

        // Po nalezení nových slotů do nich hned načteme data
        if (saveSystem != null)
        {
            saveSystem.LoadInventory();
        }
    }

    // 🔍 VYLEPŠENÉ HLEDÁNÍ (Funguje i na skryté objekty uvnitř menu)
    void FindUIReferences()
    {
        // 1. Zkusíme získat Inventář Panel z CentralMenuUI (nejspolehlivější cesta)
        if (inventoryUI == null && CentralMenuUI.Instance != null)
        {
            inventoryUI = CentralMenuUI.Instance.inventoryPanel;
        }

        // Záložní plán: Kdyby CentralMenuUI neexistovalo, zkusíme najít panel tagem (pokud je aktivní)
        if (inventoryUI == null)
        {
            GameObject panel = GameObject.FindGameObjectWithTag("InventoryPanel");
            if (panel != null) inventoryUI = panel;
        }

        // 2. Pokud máme panel, prohledáme jeho vnitřek (Děti), abychom našli zbytek
        // Používáme GetComponentsInChildren<Transform>(true), kde true = "včetně neaktivních"
        if (inventoryUI != null)
        {
            Transform[] allChildren = inventoryUI.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allChildren)
            {
                // Hledání slotů (Rodič slotů)
                if (t.CompareTag("InventorySlotsParent"))
                {
                    itemSlots = t.GetComponentsInChildren<ItemSlot>(true);
                }
                // Hledání Popisků
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
        else
        {
            // Debug.LogWarning("InventoryManager: Nemůžu najít InventoryPanel! Ujisti se, že CentralMenuUI funguje.");
        }
    }

    // --- Zbytek logiky zůstává beze změny ---

    public void ToggleInventory()
    {
        menuActivated = !menuActivated;
        if(inventoryUI) inventoryUI.SetActive(menuActivated);
        Time.timeScale = menuActivated ? 0 : 1;
    }

    public void OpenInventory()
    {
        menuActivated = true;
        if(inventoryUI) inventoryUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseInventory()
    {
        menuActivated = false;
        if(inventoryUI) inventoryUI.SetActive(false);
        Time.timeScale = 1;
    }

    public int AddItem(ItemSO itemData, int quantity)
    {
        if (itemSlots == null) return quantity;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].itemData == null || itemSlots[i].itemData == itemData)
            {
                int leftOver = itemSlots[i].AddItem(itemData, quantity);
                if (leftOver > 0) quantity = leftOver;
                else return 0;
            }
        }
        return quantity;
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
            if(slot != null && slot.selectedShader != null)
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

    public void RemoveItem(ItemSO item, int amount)
    {
        if (itemSlots == null) return;
        foreach (var slot in itemSlots)
        {
            if (slot.itemData == item)
            {
                int remove = Mathf.Min(slot.quantity, amount);
                slot.RemoveItem(remove);
                amount -= remove;
                if (amount <= 0) return;
            }
        }
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