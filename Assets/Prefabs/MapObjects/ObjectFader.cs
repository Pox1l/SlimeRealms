using UnityEngine;
using System.Collections;

public class ObjectFader : MonoBehaviour
{
    [Header("Settings")]
    public float fadeSpeed = 10f;
    public float targetAlpha = 0.4f;

    private SpriteRenderer spriteRenderer;
    private float initialAlpha;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            initialAlpha = spriteRenderer.color.a;
        }
    }

    // Tady už není Update(), takže to nežere výkon naprázdno!

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(targetAlpha));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(initialAlpha));
        }
    }

    // Coroutine bìží jen chvilku, dokud se barva nezmìní, pak se vypne
    IEnumerator FadeTo(float target)
    {
        if (spriteRenderer == null) yield break;

        float currentAlpha = spriteRenderer.color.a;

        while (Mathf.Abs(currentAlpha - target) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, target, fadeSpeed * Time.deltaTime);

            Color c = spriteRenderer.color;
            c.a = currentAlpha;
            spriteRenderer.color = c;

            yield return null; // Poèká na další snímek
        }

        // Ujištìní, že je to pøesnì na cílové hodnotì
        Color finalColor = spriteRenderer.color;
        finalColor.a = target;
        spriteRenderer.color = finalColor;
    }
}