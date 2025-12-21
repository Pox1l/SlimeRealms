using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Nastavení")]
    public float knockbackForce = 50f; // Zvednuto na 50
    public float knockbackTime = 0.2f;

    [Header("Reference (Přetáhni ručně!)")]
    public Rigidbody2D rb;
    public MonoBehaviour playerMovementScript;

    private void Awake()
    {
        // Zkusíme najít automaticky, ale vypíšeme, jestli se to povedlo
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerMovementScript == null) playerMovementScript = GetComponent<PlayerMovement>();
    }

    public void ApplyKnockback(Transform damageSource)
    {
        Debug.Log("1. 🟢 ApplyKnockback byl zavolán!");

        if (rb == null)
        {
            Debug.LogError("2. 🔴 CHYBA: Chybí Rigidbody2D! Přetáhni ho v Inspectoru.");
            return;
        }

        if (playerMovementScript == null)
        {
            Debug.LogError("2. 🔴 CHYBA: Chybí PlayerMovement script! Přetáhni ho v Inspectoru.");
        }

        if (damageSource == null)
        {
            Debug.LogError("2. 🔴 CHYBA: Nevím, odkud rána přišla (damageSource je null).");
            return;
        }

        //Debug.Log("3. 🟢 Všechny reference OK. Spouštím odhození.");
        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(damageSource));
    }

    IEnumerator KnockbackRoutine(Transform damageSource)
    {
        // 1. Vypnout pohyb
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
           // Debug.Log("4. 🟢 Pohyb hráče vypnut.");
        }

        // 2. Aplikovat sílu
        rb.velocity = Vector2.zero;
        Vector2 direction = (transform.position - damageSource.position).normalized;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        //Debug.Log($"5. 🟢 Síla aplikována: {direction * knockbackForce}");

        // 3. Čekat
        yield return new WaitForSeconds(knockbackTime);

        // 4. Zapnout pohyb
        rb.velocity = Vector2.zero;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        //Debug.Log("6. 🟢 Konec knockbacku, pohyb zapnut.");
    }
}