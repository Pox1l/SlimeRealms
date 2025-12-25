// PickupNotificationEntry - Úprava pro Pooling
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

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Sprite icon, string message, bool isError = false)
    {
        // Ujistíme se, že je objekt aktivní a na začátku seznamu
        gameObject.SetActive(true);
        transform.SetAsFirstSibling();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (text != null)
        {
            text.text = message;
            text.color = isError ? Color.red : Color.white;
        }

        // Reset stavu (důležité při recyklaci)
        canvasGroup.alpha = 1f;

        // Zastavit předchozí coroutiny (pro jistotu) a spustit novou
        StopAllCoroutines();
        StartCoroutine(LifeRoutine());
    }

    IEnumerator LifeRoutine()
    {
        // 1. Čekání (nezávislé na TimeScale)
        yield return new WaitForSecondsRealtime(lifeTime);

        // 2. Fade out (nezávislé na TimeScale)
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - (t / fadeTime);
            canvasGroup.alpha = k;
            yield return null;
        }

        // 3. POOLING: Místo Destroy objekt jen deaktivujeme
        gameObject.SetActive(false);
    }
}