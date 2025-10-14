using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slots")]
    public ItemSlot[] itemSlots;

    [Header("Description UI")]
    public Image descriptionIcon;
    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private bool menuActivated = false;
    public GameObject inventoryUI;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            menuActivated = !menuActivated;
            inventoryUI.SetActive(menuActivated);
            Time.timeScale = menuActivated ? 0 : 1;
        }
    }

    public int AddItem(ItemSO itemData, int quantity)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            // když je slot prázdný nebo obsahuje stejný item
            if (itemSlots[i].itemData == null || itemSlots[i].itemData == itemData)
            {
                int leftOver = itemSlots[i].AddItem(itemData, quantity);
                if (leftOver > 0)
                {
                    // pokud zbylo, zkus další slot
                    quantity = leftOver;
                }
                else
                {
                    return 0;
                }
            }
        }
        return quantity;
    }

    public void ShowItemDescription(ItemSO item)
    {
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
        foreach (var slot in itemSlots)
        {
            slot.selectedShader.SetActive(false);
        }
    }
}
