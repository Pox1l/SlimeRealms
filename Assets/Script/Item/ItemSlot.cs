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

    // 🔥 ODSTRANĚNO: Proměnné pro časování (lastClickTime, CLICK_COOLDOWN)

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

        // 🔥 ODSTRANĚNO: Kontrola času (cooldown) je pryč.

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

        // --- Pravé tlačítko (Kontextové Menu) ---
        if (eventData.button == PointerEventData.InputButton.Right && itemData != null)
        {
            // Místo přímého smazání zavoláme menu
            if (InventoryManager.Instance.contextMenu != null)
            {
                InventoryManager.Instance.contextMenu.OpenMenu(this, eventData.position);
            }
            else
            {
                Debug.LogWarning("Není přiřazeno ContextMenu v InventoryManageru!");
            }
        }
    }
}