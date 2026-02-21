using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryContextMenu : MonoBehaviour
{
    public static InventoryContextMenu Instance;

    [Header("Nastavení")]
    public float closeDistance = 200f;

    public GameObject activePanel;
    private ItemSlot currentSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void RegisterSceneUI(ContextMenuSceneUI ui)
    {
        activePanel = ui.menuPanel;

        if (ui.useButton != null)
        {
            ui.useButton.onClick.RemoveAllListeners();
            ui.useButton.onClick.AddListener(OnUseItem);
        }

        // 🔥 REGISTRACE EQUIP TLAČÍTKA
        if (ui.equipButton != null)
        {
            ui.equipButton.onClick.RemoveAllListeners();
            ui.equipButton.onClick.AddListener(OnEquipToQuickSlot);
        }

        if (ui.deleteButton != null)
        {
            ui.deleteButton.onClick.RemoveAllListeners();
            ui.deleteButton.onClick.AddListener(OnDeleteItem);
        }

        if (activePanel != null) activePanel.SetActive(false);
    }

    private void Update()
    {
        if (activePanel == null || !activePanel.activeSelf) return;

        if (Vector2.Distance(Input.mousePosition, activePanel.transform.position) > closeDistance)
        {
            CloseMenu();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            CloseMenu();
        }
    }

    public void OpenMenu(ItemSlot slot, Vector2 mousePosition)
    {
        if (activePanel == null) return;
        currentSlot = slot;
        activePanel.SetActive(true);
        activePanel.transform.position = mousePosition;
        activePanel.transform.SetAsLastSibling();
    }

    public void CloseMenu()
    {
        if (activePanel != null) activePanel.SetActive(false);
        currentSlot = null;
    }

    private void OnUseItem()
    {
        if (currentSlot?.itemData != null)
        {
            // OPRAVA ZDE: Přidáno zachycení hlášky failMessage
            string failMessage;
            bool used = currentSlot.itemData.UseItem(out failMessage);

            if (used)
            {
                // 🔥 Smaže item PŘÍMO z tohoto slotu
                currentSlot.RemoveItem(1);

                if (InventoryManager.Instance != null && InventoryManager.Instance.saveSystem != null)
                {
                    InventoryManager.Instance.saveSystem.SaveInventory();
                }
            }
            else if (!string.IsNullOrEmpty(failMessage))
            {
                // Prozatím vypíše chybovou hlášku do konzole (např. když máš plné HP)
                Debug.Log(failMessage);
            }
        }
        CloseMenu();
    }

    // 🔥 NOVÁ METODA: PŘIŘAZENÍ DO RYCHLÉHO SLOTU 🔥
    private void OnEquipToQuickSlot()
    {
        if (currentSlot == null || currentSlot.itemData == null)
        {
            CloseMenu();
            return;
        }

        // 🔥 KONTROLA: Je item označený jako "použitelný"?
        if (currentSlot.itemData.isUsable)
        {
            if (QuickSlotManager.Instance != null)
            {
                QuickSlotManager.Instance.AssignItemToSlot(currentSlot.itemData);
            }
        }
        else
        {
            Debug.Log($"❌ Item '{currentSlot.itemData.itemName}' nelze dát do Quick Slotu (není Usable).");
            // Sem můžeš přidat třeba zvuk chyby nebo vyskakovací hlášku pro hráče
        }

        CloseMenu();
    }

    private void OnDeleteItem()
    {
        if (currentSlot != null)
        {
            // 🔥 Smaže celý obsah PŘÍMO z tohoto slotu
            currentSlot.RemoveItem(currentSlot.quantity);

            if (InventoryManager.Instance != null && InventoryManager.Instance.saveSystem != null)
            {
                InventoryManager.Instance.saveSystem.SaveInventory();
            }
        }
        CloseMenu();
    }
}