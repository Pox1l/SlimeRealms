using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponIconPopup : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;        // Přetáhni sem ten Image
    public CanvasGroup group;      // Přetáhni sem CanvasGroup

    [Header("Settings")]
    public float visibleTime = 0.5f; // Jak dlouho svítí naplno
    public float fadeSpeed = 2f;     // Jak rychle mizí

    private Coroutine fadeRoutine;
    private Transform camTransform;

    void Start()
    {
        if (Camera.main != null) camTransform = Camera.main.transform;

        // Na začátku skryjeme
        group.alpha = 0;
    }

    void LateUpdate()
    {
        // 🔥 BILLBOARD EFEKT 🔥
        // Zajistí, že se ikona netočí s hráčem, ale je vždy "placatá" vůči kameře
        if (camTransform != null)
        {
            transform.rotation = camTransform.rotation;
        }
    }

    public void Show(Sprite weaponIcon)
    {
        if (weaponIcon == null) return;

        // 1. Nastavíme ikonu
        iconImage.sprite = weaponIcon;

        // 2. Resetujeme viditelnost
        group.alpha = 1f;

        // 3. Spustíme odpočet pro zmizení
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutProcess());
    }

    IEnumerator FadeOutProcess()
    {
        // Chvíli počkáme (plná viditelnost)
        yield return new WaitForSeconds(visibleTime);

        // Postupné mizení
        while (group.alpha > 0f)
        {
            group.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}