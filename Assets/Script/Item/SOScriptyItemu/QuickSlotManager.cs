using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance;

    [Header("Settings")]
    public KeyCode useKey = KeyCode.Alpha3;
    public float warningDisplayTime = 2f;
    public float warningFadeTime = 0.5f;

    [Header("UI Reference")]
    public GameObject quickSlotUI;
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image cooldownOverlay;

    [SerializeField]
    public TextMeshProUGUI warningText;
    public CanvasGroup warningCanvasGroup;

    [Header("Effects")]
    public GameObject healParticleObject;

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

        if (warningText) warningText.text = "";
        if (warningCanvasGroup) warningCanvasGroup.alpha = 0f;

        if (healParticleObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform healTransform = player.transform.Find("HealParticle");
                if (healTransform != null)
                {
                    healParticleObject = healTransform.gameObject;
                }
            }
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

    public ItemSO GetCurrentItem()
    {
        return currentItem;
    }

    public void ClearSlot()
    {
        currentItem = null;
        UpdateSlotUI();
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

        if (InventoryManager.Instance != null && InventoryManager.Instance.saveSystem != null)
        {
            InventoryManager.Instance.saveSystem.SaveInventory();
        }
    }

    private void UseQuickItem()
    {
        if (InventoryManager.Instance == null) return;

        int count = InventoryManager.Instance.GetTotalItemCount(currentItem);
        if (count <= 0)
        {
            ClearSlot();
            if (InventoryManager.Instance.saveSystem != null) InventoryManager.Instance.saveSystem.SaveInventory();
            return;
        }

        string failMessage;
        bool used = currentItem.UseItem(out failMessage);

        if (used)
        {
            if (warningCanvasGroup) warningCanvasGroup.alpha = 0f;
            InventoryManager.Instance.RemoveItem(currentItem, 1);

            if (healParticleObject != null)
            {
                ParticleSystem healPS = healParticleObject.GetComponent<ParticleSystem>();
                if (healPS != null)
                {
                    healPS.Play();
                }
            }

            if (InventoryManager.Instance.GetTotalItemCount(currentItem) <= 0)
            {
                ClearSlot();
                if (InventoryManager.Instance.saveSystem != null) InventoryManager.Instance.saveSystem.SaveInventory();
            }

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

    private IEnumerator ShowAndFadeWarningRoutine()
    {
        warningCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(warningDisplayTime);

        float elapsedTime = 0f;
        while (elapsedTime < warningFadeTime)
        {
            elapsedTime += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / warningFadeTime);
            yield return null;
        }

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
                ClearSlot();
                if (InventoryManager.Instance != null && InventoryManager.Instance.saveSystem != null)
                    InventoryManager.Instance.saveSystem.SaveInventory();
            }
        }
    }
}