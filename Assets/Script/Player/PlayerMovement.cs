using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashEnergyCost = 25f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing = false;

    public MMF_Player dashFeedback;

    [Header("Animation")]
    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Pokud dashujeme, ignorujeme input
        if (isDashing) return;

        // Načtení pohybu
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        // 🔥 Kontrola staminy a spuštění Dashe
        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector2.zero)
        {
            // Zeptáme se Banky (PlayerStats), jestli máme dost energie
            if (PlayerStats.Instance != null && PlayerStats.Instance.HasStamina(dashEnergyCost))
            {
                StartCoroutine(Dash());
            }
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Hýbeme hráčem pouze pokud nedashuje
        if (!isDashing)
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        // 🔥 Utratíme staminu
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.UseStamina(dashEnergyCost);
        }

        // Aplikace rychlosti pro dash
        rb.velocity = movement * dashSpeed;

        // Efekt (Feedbacks)
        if (dashFeedback != null)
        {
            dashFeedback.PlayFeedbacks();
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        // Na konci dashe zastavíme setrvačnost (pokud stále běžíme)
        rb.velocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    // 🔥 DŮLEŽITÁ OPRAVA PRO KNOCKBACK 🔥
    // Tato funkce se zavolá automaticky, když PlayerKnockback vypne tento skript (enabled = false).
    private void OnDisable()
    {
        // 1. Okamžitě zastavíme Dash coroutinu, aby nepřepsala fyziku odhození
        StopAllCoroutines();

        // 2. Resetujeme stav, abychom po zapnutí nebyly zaseklí v "isDashing"
        isDashing = false;

        // Poznámka: Nenastavujeme velocity na nulu, protože chceme, aby nás síla knockbacku odhodila.
    }
}