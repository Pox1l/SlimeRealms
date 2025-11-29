using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    public ItemSO itemData;
    public int quantity;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    public GameObject selectedShader;

    private Canvas parentCanvas;
    private GameObject dragIcon;
    private Image dragImage;
    private bool isDragging = false;

    // 🔥 PŘIDÁNO: Proměnná pro čas poslední akce
    private float lastClickTime;
    // 🔥 PŘIDÁNO: Jak dlouho se musí čekat (0.2 sekundy stačí)
    private const float CLICK_COOLDOWN = 1f;

    private InventorySaveSystem SaveSystem
    {
        get
        {
            GameObject saveObj = GameObject.FindGameObjectWithTag("itemSaveSys");
            if (saveObj != null) return saveObj.GetComponent<InventorySaveSystem>();
            return null;
        }
    }

    public bool IsFull => itemData != null && quantity >= itemData.maxStack;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        UpdateUI();
    }

    public int AddItem(ItemSO newItem, int amount)
    {
        if (itemData == null)
        {
            itemData = newItem;
            itemImage.sprite = newItem.icon;
        }

        if (itemData != newItem) return amount;

        quantity += amount;
        if (quantity > itemData.maxStack)
        {
            int leftover = quantity - itemData.maxStack;
            quantity = itemData.maxStack;
            UpdateUI();
            return leftover;
        }

        UpdateUI();
        return 0;
    }

    public void RemoveItem(int amount)
    {
        if (itemData == null) return;

        quantity -= amount;
        if (quantity <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (itemData != null)
        {
            itemImage.enabled = true;
            itemImage.sprite = itemData.icon;
            quantityText.enabled = quantity > 1;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
            itemImage.enabled = false;
            quantityText.enabled = false;
            quantityText.text = "";
        }
    }

    private void ClearSlot()
    {
        itemData = null;
        quantity = 0;
        UpdateUI();
    }

    // --- DRAG AND DROP ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(parentCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        dragImage = dragIcon.AddComponent<Image>();
        dragImage.sprite = itemImage.sprite;
        dragImage.raycastTarget = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = itemImage.rectTransform.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
        isDragging = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot draggedSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
        bool changed = false;

        if (draggedSlot != null && draggedSlot != this)
        {
            if (draggedSlot.itemData == itemData && itemData != null)
            {
                int leftover = AddItem(itemData, draggedSlot.quantity);
                if (leftover <= 0) draggedSlot.ClearSlot();
                else { draggedSlot.quantity = leftover; draggedSlot.UpdateUI(); }
                changed = true;
            }
            else
            {
                SwapItems(draggedSlot);
                changed = true;
            }
        }

        if (changed && SaveSystem != null)
        {
            SaveSystem.SaveInventory();
        }
    }

    private void SwapItems(ItemSlot other)
    {
        ItemSO tempData = other.itemData;
        int tempQuantity = other.quantity;

        other.itemData = this.itemData;
        other.quantity = this.quantity;
        other.UpdateUI();

        this.itemData = tempData;
        this.quantity = tempQuantity;
        this.UpdateUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        // 🔥 OCHRANA PROTI SPAMOVÁNÍ:
        // Pokud od posledního kliknutí uběhlo méně než 0.2 sekundy, ignorujeme to.
        // Používáme unscaledTime, protože timeScale může být 0 (pause menu).
        if (Time.unscaledTime - lastClickTime < CLICK_COOLDOWN)
        {
            return;
        }

        // Uložíme čas tohoto kliknutí
        lastClickTime = Time.unscaledTime;

        // --- Levé tlačítko (Info) ---
        if (eventData.button == PointerEventData.InputButton.Left && itemData != null)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.DeselectAllSlots();
                InventoryManager.Instance.ShowItemDescription(itemData);
            }
            if (selectedShader) selectedShader.SetActive(true);
        }

        // --- Pravé tlačítko (Mazání) ---
        if (eventData.button == PointerEventData.InputButton.Right && itemData != null)
        {
            Debug.Log("Item odstraněn: " + itemData.itemName);
            ClearSlot();

            if (SaveSystem != null)
            {
                SaveSystem.SaveInventory();
            }
        }
    }
}