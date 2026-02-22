using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PickupNotificationEntry : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI text;

    [Header("Nastavení")]
    public float lifeTime = 1.5f;
    public float fadeTime = 0.5f;

    private CanvasGroup canvasGroup;

    [HideInInspector] public string baseMessage;
    private int currentAmount = 0; // Kolik celkem předmětů se sebralo
    private int messageCount = 1;  // Kolikrát se ukázal error (např. plný inv)
    private bool isItemPickup = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // --- LOGIKA PRO SEBRÁNÍ PŘEDMĚTU ---
    public void SetupPickup(Sprite icon, string itemName, int amount)
    {
        isItemPickup = true;
        baseMessage = itemName; // Uložíme si čisté jméno (např. "Wood")
        currentAmount = amount; // První sebraný počet

        InitializeUI(icon, false);
    }

    public void AddPickupAmount(int amount)
    {
        currentAmount += amount; // Přičteme nově sebrané kusy ke starým
        UpdateText(false);
        ResetTimer();
    }

    // --- LOGIKA PRO OBECNÉ ZPRÁVY (Error, Plný batoh) ---
    public void SetupMessage(Sprite icon, string message, bool isError = false)
    {
        isItemPickup = false;
        baseMessage = message;
        messageCount = 1;

        InitializeUI(icon, isError);
    }

    public void AddMessageCount(bool isError = true)
    {
        messageCount++;
        UpdateText(isError);
        ResetTimer();
    }

    // --- SPOLEČNÉ METODY ---
    private void InitializeUI(Sprite icon, bool isError)
    {
        gameObject.SetActive(true);
        transform.SetAsFirstSibling();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        UpdateText(isError);
        ResetTimer();
    }

    private void UpdateText(bool isError)
    {
        if (text == null) return;

        if (isItemPickup)
        {
            // Píše např.: "+5 Wood"
            text.text = currentAmount > 0 ? $"+{currentAmount} {baseMessage}" : baseMessage;
        }
        else
        {
            // Píše např.: "3x Inventory Full!"
            text.text = messageCount > 1 ? $"{messageCount}x {baseMessage}" : baseMessage;
        }

        text.color = isError ? Color.red : Color.white;
    }

    private void ResetTimer()
    {
        canvasGroup.alpha = 1f;
        StopAllCoroutines();
        StartCoroutine(LifeRoutine());
    }

    IEnumerator LifeRoutine()
    {
        yield return new WaitForSecondsRealtime(lifeTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - (t / fadeTime);
            canvasGroup.alpha = k;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}