using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler, // Přidáno pro detekci najetí myší
    IPointerExitHandler   // Přidáno pro detekci odjetí myší
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

    // Statická proměnná pro sledování, který slot je aktuálně "kliknutý" (vybraný)
    public static ItemSlot currentSelectedSlot;

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

        // 🔥 PŘIDÁNO: Zhasnutí pozadí a zrušení výběru
        if (selectedShader != null)
            selectedShader.SetActive(false);

        if (currentSelectedSlot == this)
        {
            currentSelectedSlot = null;
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.ShowItemDescription(null);
        }

        UpdateUI();
    }

    // --- HOVER LOGIKA (Najetí myší) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        // Pokud je item platný, pošleme ho manažeru k zobrazení.
        if (itemData != null && InventoryManager.Instance != null)
        {
            // Voláme TryShowSelectedDescription, aby se zohlednil i Selected Slot
            InventoryManager.Instance.TryShowSelectedDescription(itemData);

            // 🔥 Zobrazí ikonku RightClick u kurzoru
            InventoryManager.Instance.ShowRightClickHint(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Pošleme manažeru null, aby věděl, že už není aktivní hover.
        // Manažer pak zkontroluje, jestli má ukázat popis vybraného slotu.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.TryShowSelectedDescription(null);

            // 🔥 Skryje ikonku RightClick při odjetí ze slotu
            InventoryManager.Instance.ShowRightClickHint(false);
        }
    }

    // --- KLIKNUTÍ (Výběr) ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        // --- Levé tlačítko (Výběr itemu) ---
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (itemData != null)
            {
                SelectThisSlot();
            }
            else
            {
                // Kliknutí do prázdna zruší výběr
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.DeselectAllSlots();
                    InventoryManager.Instance.ShowItemDescription(null);
                }
                currentSelectedSlot = null;
            }
        }

        // --- Pravé tlačítko (Kontextové Menu) ---
        if (eventData.button == PointerEventData.InputButton.Right && itemData != null)
        {
            SelectThisSlot(); // 🔥 PŘIDÁNO: Slot se hned vybarví a označí jako vybraný

            if (InventoryManager.Instance.contextMenu != null)
            {
                InventoryManager.Instance.contextMenu.OpenMenu(this, eventData.position);

                // 🔥 Skryje ikonku RightClick, když už se otevřelo menu
                InventoryManager.Instance.ShowRightClickHint(false);
            }
            else
            {
                Debug.LogWarning("Není přiřazeno ContextMenu v InventoryManageru!");
            }
        }
    }

    // Pomocná metoda pro označení tohoto slotu
    private void SelectThisSlot()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.DeselectAllSlots(); // Zhasne starý
                                                          // Nyní voláme přímo ShowItemDescription, protože jsme klikli
            InventoryManager.Instance.ShowItemDescription(itemData);
        }

        currentSelectedSlot = this; // Nastaví tento jako aktuální
        if (selectedShader) selectedShader.SetActive(true); // Rozsvítí tento
    }

    // --- DRAG AND DROP ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;

        // 🔥 Při začátku tahání raději taky skryjeme RightClick hint
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ShowRightClickHint(false);

        // Vytvoření ikonky pro tažení
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
                // Stohování (Stacking)
                int leftover = AddItem(itemData, draggedSlot.quantity);
                if (leftover <= 0) draggedSlot.ClearSlot();
                else { draggedSlot.quantity = leftover; draggedSlot.UpdateUI(); }
                changed = true;
            }
            else
            {
                // Prohození (Swapping)
                SwapItems(draggedSlot);
                changed = true;
            }

            // --- KLÍČOVÁ ZMĚNA: Přesun výběru na nový slot ---
            // Po dokončení dropu chceme, aby byl vybraný TENTO slot (kam jsme item pustili),
            // nikoliv ten starý (odkud jsme ho vzali).
            if (this.itemData != null)
            {
                SelectThisSlot();
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
}