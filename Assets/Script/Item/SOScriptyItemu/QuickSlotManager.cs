using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance;

    [Header("Settings")]
    public KeyCode useKey = KeyCode.Alpha3;

    [Header("UI Reference")]
    public GameObject quickSlotUI;
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image cooldownOverlay;  // 🖼️ Ten tmavý kruh

    private ItemSO currentItem;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateSlotUI;
        }
        UpdateSlotUI();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateSlotUI;
        }
    }

    private void Update()
    {
        // 1. 🔥 VYKRESLOVÁNÍ COOLDOWNU (Inspirováno PlayerCombatUI)
        if (currentItem != null && cooldownOverlay != null)
        {
            // Kdy bude item znovu připraven? (Jako nextAttackTime)
            float readyTime = currentItem.lastTimeUsed + currentItem.cooldown;

            // Kolik času zbývá do připravenosti?
            float timeLeft = readyTime - Time.time;

            if (timeLeft > 0)
            {
                // Vypočítáme procento (0 až 1)
                cooldownOverlay.fillAmount = timeLeft / currentItem.cooldown;
            }
            else
            {
                // Cooldown skončil
                cooldownOverlay.fillAmount = 0;
            }
        }

        // 2. Použití klávesy
        if (Input.GetKeyDown(useKey))
        {
            if (currentItem != null) UseQuickItem();
        }
    }

    public void AssignItemToSlot(ItemSO item)
    {
        currentItem = item;
        UpdateSlotUI();
    }

    private void UseQuickItem()
    {
        if (InventoryManager.Instance == null) return;

        // Kontrola počtu
        int count = InventoryManager.Instance.GetTotalItemCount(currentItem);
        if (count <= 0)
        {
            currentItem = null;
            UpdateSlotUI();
            return;
        }

        // Zkusíme použít item
        bool used = currentItem.UseItem();

        if (used)
        {
            InventoryManager.Instance.RemoveItem(currentItem, 1);
            UpdateSlotUI();
        }
    }

    private void UpdateSlotUI()
    {
        if (quickSlotUI) quickSlotUI.SetActive(true);

        if (currentItem == null)
        {
            if (iconImage) { iconImage.sprite = null; iconImage.enabled = false; }
            if (countText) countText.text = "";
            if (cooldownOverlay) cooldownOverlay.fillAmount = 0;
        }
        else
        {
            if (iconImage) { iconImage.sprite = currentItem.icon; iconImage.enabled = true; }

            int count = 0;
            if (InventoryManager.Instance != null)
                count = InventoryManager.Instance.GetTotalItemCount(currentItem);

            if (countText) countText.text = count > 0 ? count.ToString() : "";

            if (count <= 0)
            {
                currentItem = null;
                UpdateSlotUI();
            }
        }
    }
}