using UnityEngine;
using System.Collections.Generic;
using FMODUnity; // 1. PŘIDÁNO: Nutné pro FMOD

public class CrystalVisualController : MonoBehaviour
{
    [Header("Komponenty")]
    public SpriteRenderer crystalRenderer;

    [Header("Vzhledy pro každou stage")]
    public List<Sprite> crystalSprites = new List<Sprite>();

    [Header("Efekty (Volitelné)")]
    public ParticleSystem repairEffect;

    [Header("Audio (FMOD)")]
    public EventReference repairSound; // 2. PŘIDÁNO: Políčko pro výběr zvuku v Inspektoru

    void Awake()
    {
        if (crystalRenderer == null)
        {
            crystalRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void UpdateVisuals(int stageIndex)
    {
        Debug.Log($"🔮 CrystalVisualController: Pokus o změnu vzhledu na Stage {stageIndex}");

        if (crystalRenderer == null) return;

        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.enabled)
        {
            anim.enabled = false;
        }

        if (crystalSprites.Count == 0) return;

        Sprite finalSprite;
        if (stageIndex >= crystalSprites.Count)
        {
            finalSprite = crystalSprites[crystalSprites.Count - 1];
        }
        else
        {
            finalSprite = crystalSprites[stageIndex];
        }

        crystalRenderer.sprite = finalSprite;
    }

    public void PlayRepairEffect()
    {
        // 1. Spustí vizuální efekt
        if (repairEffect != null)
        {
            repairEffect.Stop();
            repairEffect.Play();
        }

        // 3. PŘIDÁNO: Spustí FMOD zvuk
        if (!repairSound.IsNull)
        {
            // Pustí zvuk na pozici krystalu (transform.position) pro 3D efekt
            RuntimeManager.PlayOneShot(repairSound, transform.position);
        }
    }
}