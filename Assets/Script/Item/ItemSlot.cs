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

    public bool IsFull => itemData != null && quantity >= itemData.maxStack;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        UpdateUI();
    }

    // ✅ Přidání itemu do slotu
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

    // ✅ Odebrání itemů
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

    private void UpdateUI()
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

    // -----------------------
    // 👇 Drag & Drop logika
    // -----------------------
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

        if (draggedSlot != null && draggedSlot != this)
        {
            // stejné itemy -> složit do stacku
            if (draggedSlot.itemData == itemData && itemData != null)
            {
                int leftover = AddItem(itemData, draggedSlot.quantity);
                if (leftover <= 0)
                {
                    draggedSlot.ClearSlot();
                }
                else
                {
                    draggedSlot.quantity = leftover;
                    draggedSlot.UpdateUI();
                }
            }
            else
            {
                SwapItems(draggedSlot);
            }
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

    // -----------------------
    // ✅ Klikání
    // -----------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        // pokud probíhá drag, ignoruj kliknutí
        if (isDragging) return;

        if (eventData.button == PointerEventData.InputButton.Left && itemData != null)
        {
            InventoryManager.Instance.DeselectAllSlots();
            selectedShader.SetActive(true);

            // ⚡ ukázat popis
            InventoryManager.Instance.ShowItemDescription(itemData);
        }

        if (eventData.button == PointerEventData.InputButton.Right && itemData != null)
        {
            // Tady dej logiku pro pravý klik (např. split stacku, použít item, atd.)
            Debug.Log("Right click on " + itemData.name);
        }
    }
}
