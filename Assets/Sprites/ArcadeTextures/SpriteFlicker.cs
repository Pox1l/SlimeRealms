using UnityEngine;

public class SpriteFlicker : MonoBehaviour
{
    [Header("Průhlednost (0 až 255)")]
    [Range(0, 255)] public int minAlpha = 40;
    [Range(0, 255)] public int maxAlpha = 100;

    [Header("Rychlost")]
    [Tooltip("Nejpomalejší možné blikání")]
    public float minSpeed = 2f;

    [Tooltip("Nejrychlejší možné blikání")]
    public float maxSpeed = 10f;

    // Skutečná rychlost tohoto konkrétního světla (vylosuje se při startu)
    private float actualSpeed;

    private SpriteRenderer spriteRenderer;
    private float targetAlpha01;
    private Color baseRGB;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (spriteRenderer != null)
        {
            baseRGB = spriteRenderer.color;

            // 🔥 Tady se vylosuje unikátní rychlost pro tento jeden automat
            actualSpeed = Random.Range(minSpeed, maxSpeed);

            PickNewTarget();
        }
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        float currentAlpha01 = spriteRenderer.color.a;

        // Používáme vylosovanou 'actualSpeed'
        float newAlpha01 = Mathf.MoveTowards(currentAlpha01, targetAlpha01, Time.deltaTime * actualSpeed);

        spriteRenderer.color = new Color(baseRGB.r, baseRGB.g, baseRGB.b, newAlpha01);

        if (Mathf.Abs(newAlpha01 - targetAlpha01) < 0.01f)
        {
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        float min01 = minAlpha / 255f;
        float max01 = maxAlpha / 255f;
        targetAlpha01 = Random.Range(min01, max01);
    }
}