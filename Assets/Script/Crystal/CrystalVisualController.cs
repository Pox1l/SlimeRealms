using UnityEngine;
using System.Collections.Generic;

public class CrystalVisualController : MonoBehaviour
{
    [Header("Komponenty")]
    public SpriteRenderer crystalRenderer; // Sem pøetáhni SpriteRenderer krystalu

    [Header("Vzhledy pro každou stage")]
    // Sem nahaž sprity: Index 0 = rozbitý, Index 1 = opravený 1. stupeò, atd.
    public List<Sprite> crystalSprites = new List<Sprite>();

    [Header("Efekty (Volitelné)")]
    public ParticleSystem repairEffect; // Efekt, který se pøehraje pøi opravì

    /// <summary>
    /// Zmìní sprite krystalu podle aktuální stage.
    /// </summary>
    public void UpdateVisuals(int stageIndex)
    {
        if (crystalRenderer == null) return;

        if (crystalSprites.Count == 0)
        {
            Debug.LogWarning("CrystalVisualController: Nemáš nastavené žádné sprity v listu!");
            return;
        }

        // Pokud je stageIndex vìtší než poèet obrázkù, použijeme ten poslední (plnì opravený)
        if (stageIndex >= crystalSprites.Count)
        {
            crystalRenderer.sprite = crystalSprites[crystalSprites.Count - 1];
        }
        else
        {
            crystalRenderer.sprite = crystalSprites[stageIndex];
        }
    }

    /// <summary>
    /// Pøehraje efekt opravy (jiskry, záblesk).
    /// </summary>
    public void PlayRepairEffect()
    {
        if (repairEffect != null)
        {
            repairEffect.Stop(); // Reset, kdyby zrovna bìžel
            repairEffect.Play();
        }
    }
}