using UnityEngine;
using System.Collections.Generic;

public class CrystalVisualController : MonoBehaviour
{
    [Header("Komponenty")]
    public SpriteRenderer crystalRenderer; // Pokud necháš prázdné, zkusí se najít samo

    [Header("Vzhledy pro každou stage")]
    // Sem nahaž sprity: Index 0 = rozbitý, Index 1 = opravený 1. stupeň, atd.
    public List<Sprite> crystalSprites = new List<Sprite>();

    [Header("Efekty (Volitelné)")]
    public ParticleSystem repairEffect;

    void Awake()
    {
        // 🛠️ Automatická oprava: Pokud chybí reference, najdi ji na stejném objektu
        if (crystalRenderer == null)
        {
            crystalRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Změní sprite krystalu podle aktuální stage.
    /// </summary>
    public void UpdateVisuals(int stageIndex)
    {
        // 1. DEBUG VÝPIS - Pokud se toto neobjeví v konzoli, nemáš propojené skripty!
        Debug.Log($"🔮 CrystalVisualController: Pokus o změnu vzhledu na Stage {stageIndex}");

        if (crystalRenderer == null)
        {
            Debug.LogError("❌ CHYBA: CrystalVisualController nemá přiřazený SpriteRenderer!");
            return;
        }

        // 2. KONTROLA ANIMATORU - Animator blokuje změnu spritů
        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.enabled)
        {
            Debug.LogWarning("⚠️ VAROVÁNÍ: Na Krystalu běží Animator! Vypínám ho, aby šel změnit Sprite.");
            anim.enabled = false; // Vypneme Animator, aby nám nepřepisoval obrázek
        }

        // 3. Kontrola seznamu spritů
        if (crystalSprites.Count == 0)
        {
            Debug.LogWarning("⚠️ VAROVÁNÍ: Nemáš v Inspectoru nastavené žádné obrázky (Crystal Sprites)!");
            return;
        }

        // 4. Změna obrázku
        Sprite finalSprite;

        if (stageIndex >= crystalSprites.Count)
        {
            // Pokud je stage vyšší než počet obrázků, dej tam ten poslední (úplně opravený)
            finalSprite = crystalSprites[crystalSprites.Count - 1];
        }
        else
        {
            finalSprite = crystalSprites[stageIndex];
        }

        crystalRenderer.sprite = finalSprite;
        Debug.Log($"✅ Krystal změněn na obrázek: {finalSprite.name}");
    }

    public void PlayRepairEffect()
    {
        if (repairEffect != null)
        {
            repairEffect.Stop();
            repairEffect.Play();
        }
    }
}