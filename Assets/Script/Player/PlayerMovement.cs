using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using FMODUnity; // 🔥 PŘIDÁNO: Knihovna pro FMOD

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashEnergyCost = 25f;
    public ParticleSystem startDashParticle;

    [Header("Audio (FMOD)")] // 🔥 PŘIDÁNO: Sekce pro zvuky
    [Tooltip("Zvuk, který se přehraje při startu dashe (např. Whoosh)")]
    public EventReference dashSound;

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

        // Kontrola staminy a spuštění Dashe
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

        // --- 🔥 AUDIO START: Přehrajeme zvuk dashe ---
        if (!dashSound.IsNull)
        {
            // Hrajeme zvuk na pozici hráče (pro případný 3D efekt)
            RuntimeManager.PlayOneShot(dashSound, transform.position);
        }
        // --- AUDIO END ---

        // Vypočítáme rotaci partiklů
        if (startDashParticle != null && movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x);
            angle = angle * Mathf.Rad2Deg;
            startDashParticle.transform.rotation = Quaternion.Euler(angle - 0f, -90f, 90f);
        }

        // Spuštění particle efektu
        if (startDashParticle != null)
        {
            startDashParticle.Play();
        }

        // Utratíme staminu
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

    // DŮLEŽITÁ OPRAVA PRO KNOCKBACK
    private void OnDisable()
    {
        // 1. Okamžitě zastavíme Dash coroutinu, aby nepřepsala fyziku odhození
        StopAllCoroutines();

        // 2. Resetujeme stav, abychom po zapnutí nebyly zaseklí v "isDashing"
        isDashing = false;
    }
}