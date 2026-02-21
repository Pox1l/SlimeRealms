using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance;

    [Header("Settings")]
    public KeyCode useKey = KeyCode.Alpha3;
    public float warningDisplayTime = 2f; // Čas zobrazení hlášky před tím, než začne mizet
    public float warningFadeTime = 0.5f;  // Jak dlouho trvá, než text úplně zmizí

    [Header("UI Reference")]
    public GameObject quickSlotUI;
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image cooldownOverlay;

    [SerializeField]
    public TextMeshProUGUI warningText;
    public CanvasGroup warningCanvasGroup; // PŘIDÁNO: Reference na Canvas Group

    private ItemSO currentItem;
    private Coroutine warningCoroutine;

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

        // PŘIDÁNO: Na začátku text zneviditelníme
        if (warningText) warningText.text = "";
        if (warningCanvasGroup) warningCanvasGroup.alpha = 0f;

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
        if (currentItem != null && cooldownOverlay != null)
        {
            float readyTime = currentItem.lastTimeUsed + currentItem.cooldown;
            float timeLeft = readyTime - Time.time;

            if (timeLeft > 0)
            {
                cooldownOverlay.fillAmount = timeLeft / currentItem.cooldown;
            }
            else
            {
                cooldownOverlay.fillAmount = 0;
            }
        }

        if (Input.GetKeyDown(useKey))
        {
            if (currentItem != null) UseQuickItem();
        }
    }

    public void AssignItemToSlot(ItemSO item)
    {
        if (!item.isUsable)
        {
            ShowWarning("Cannot be placed in Quick Slot.");
            return;
        }

        currentItem = item;
        UpdateSlotUI();
    }

    private void UseQuickItem()
    {
        if (InventoryManager.Instance == null) return;

        int count = InventoryManager.Instance.GetTotalItemCount(currentItem);
        if (count <= 0)
        {
            currentItem = null;
            UpdateSlotUI();
            return;
        }

        string failMessage;
        bool used = currentItem.UseItem(out failMessage);

        if (used)
        {
            if (warningCanvasGroup) warningCanvasGroup.alpha = 0f; // Schovat při úspěchu
            InventoryManager.Instance.RemoveItem(currentItem, 1);
            UpdateSlotUI();
        }
        else if (!string.IsNullOrEmpty(failMessage))
        {
            ShowWarning(failMessage);
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText == null || warningCanvasGroup == null) return;

        warningText.text = message;

        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(ShowAndFadeWarningRoutine());
    }

    // PŘEPRACOVÁNO: Zobrazí text a pak ho plynule skryje přes Canvas Group
    private IEnumerator ShowAndFadeWarningRoutine()
    {
        // 1. Okamžité zobrazení
        warningCanvasGroup.alpha = 1f;

        // 2. Počkání
        yield return new WaitForSeconds(warningDisplayTime);

        // 3. Plynulé mizení
        float elapsedTime = 0f;
        while (elapsedTime < warningFadeTime)
        {
            elapsedTime += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / warningFadeTime);
            yield return null; // Čeká na další frame
        }

        // 4. Úplné skrytí na konci
        warningCanvasGroup.alpha = 0f;
        warningText.text = "";
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