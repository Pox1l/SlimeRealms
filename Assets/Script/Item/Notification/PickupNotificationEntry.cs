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
    public float lifeTime = 1.5f;    // jak dlouho to zůstane viditelné
    public float fadeTime = 0.5f;    // jak dlouho mizí

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Sprite icon, string message, bool isError = false)
    {
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

        canvasGroup.alpha = 1f;
        StartCoroutine(LifeRoutine());
    }



    IEnumerator LifeRoutine()
    {
        // chvíli držet
        yield return new WaitForSeconds(lifeTime);

        // fade out
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = 1f - (t / fadeTime);
            canvasGroup.alpha = k;
            yield return null;
        }

        Destroy(gameObject);
    }
}
