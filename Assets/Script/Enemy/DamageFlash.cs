using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Nastavení")]
    public Material whiteMaterial;

    private SpriteRenderer targetSprite;
    private Material originalMaterial;
    private Coroutine flashRoutine;

    void Awake()
    {
        targetSprite = GetComponentInChildren<SpriteRenderer>();
        if (targetSprite != null)
        {
            originalMaterial = targetSprite.material;
        }
    }

   
    void OnEnable()
    {
        // Při vytažení z Poolu okamžitě resetujeme barvu a zastavíme blikání
        if (targetSprite != null && originalMaterial != null)
        {
            targetSprite.material = originalMaterial;
            flashRoutine = null;
        }
    }

    public void Flash()
    {
        if (targetSprite == null || whiteMaterial == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashProcess());
    }

    IEnumerator FlashProcess()
    {
        targetSprite.material = whiteMaterial;
        yield return new WaitForSeconds(0.1f);
        targetSprite.material = originalMaterial;
        flashRoutine = null;
    }
}