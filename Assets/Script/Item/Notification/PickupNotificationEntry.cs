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

    // 🔍 ÚPRAVA: Proměnné pro uchování základní zprávy a počítadla
    [HideInInspector] public string baseMessage;
    private int count = 1;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Sprite icon, string message, bool isError = false)
    {
        baseMessage = message; // Uložíme si původní text bez čísla
        count = 1; // Reset počítadla

        gameObject.SetActive(true);
        transform.SetAsFirstSibling();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        UpdateText(isError);

        canvasGroup.alpha = 1f;
        StopAllCoroutines();
        StartCoroutine(LifeRoutine());
    }

    // 🔍 ÚPRAVA: Nová funkce, kterou volá Manager pro navýšení počtu
    public void AddCount(bool isError = false)
    {
        count++;
        UpdateText(isError);

        // Znovu nastartujeme časovač, aby hned nezmizela
        canvasGroup.alpha = 1f;
        StopAllCoroutines();
        StartCoroutine(LifeRoutine());
    }

    // 🔍 ÚPRAVA: Samostatná funkce pro přepsání textu (přidá "Nx" na začátek, pokud je jich víc)
    private void UpdateText(bool isError)
    {
        if (text != null)
        {
            text.text = count > 1 ? $"{count}x {baseMessage}" : baseMessage;
            text.color = isError ? Color.red : Color.white;
        }
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