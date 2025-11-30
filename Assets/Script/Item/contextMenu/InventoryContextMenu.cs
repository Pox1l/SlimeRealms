using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // 🔥 DŮLEŽITÉ PRO DETEKCI KLIKNUTÍ NA UI

public class InventoryContextMenu : MonoBehaviour
{
    public static InventoryContextMenu Instance;

    [Header("Nastavení")]
    public float closeDistance = 200f; // Zvětši to klidně na 300-400, pokud je panel velký

    // Soukromé reference
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
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Registrace UI ---
    public void RegisterSceneUI(ContextMenuSceneUI ui)
    {
        activePanel = ui.menuPanel;

        if (ui.useButton != null)
        {
            ui.useButton.onClick.RemoveAllListeners();
            ui.useButton.onClick.AddListener(OnUseItem);
        }

        if (ui.deleteButton != null)
        {
            ui.deleteButton.onClick.RemoveAllListeners();
            ui.deleteButton.onClick.AddListener(OnDeleteItem);
        }

        // Pokud se scéna teprve načítá, schováme panel
        if (activePanel != null) activePanel.SetActive(false);

        Debug.Log("✅ Context Menu UI úspěšně propojeno.");
    }

    private void Update()
    {
        // Pokud menu není aktivní, nic neřešíme
        if (activePanel == null || !activePanel.activeSelf) return;

        // 1. Zavření vzdáleností (pokud myš odjede moc daleko)
        float distance = Vector2.Distance(Input.mousePosition, activePanel.transform.position);
        if (distance > closeDistance)
        {
            CloseMenu();
            return;
        }

        // 2. Zavření kliknutím mimo
        if (Input.GetMouseButtonDown(0))
        {
            // 🔥 TOTO JE TA OPRAVA 🔥
            // Pokud myš zrovna kliká na nějaký UI prvek (třeba tlačítko Smazat),
            // tak menu NEZAVÍRÁME!
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Pokud klikáme do prázdna, zavřeme menu
            CloseMenu();
        }
    }

    public void OpenMenu(ItemSlot slot, Vector2 mousePosition)
    {
        if (activePanel == null)
        {
            Debug.LogWarning("⚠️ Nemůžu otevřít menu - UI není připojeno!");
            return;
        }

        currentSlot = slot;
        activePanel.SetActive(true);
        activePanel.transform.position = mousePosition;

        // Ujistíme se, že je panel v popředí
        activePanel.transform.SetAsLastSibling();
    }

    public void CloseMenu()
    {
        if (activePanel != null) activePanel.SetActive(false);
        currentSlot = null;
    }

    private void OnUseItem()
    {
        Debug.Log($"🧪 POUŽITO: {currentSlot?.itemData?.itemName}");
        // Zde doplň logiku použití
        CloseMenu();
    }

    private void OnDeleteItem()
    {
        Debug.Log($"🗑️ SMAZÁNO: {currentSlot?.itemData?.itemName}");

        if (currentSlot != null)
        {
            currentSlot.RemoveItem(currentSlot.quantity);
            if (InventoryManager.Instance != null && InventoryManager.Instance.saveSystem != null)
            {
                InventoryManager.Instance.saveSystem.SaveInventory();
            }
        }
        CloseMenu();
    }
}