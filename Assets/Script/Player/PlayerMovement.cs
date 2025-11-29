using UnityEngine;
using System.Collections;

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

    // ❌ Energy proměnné smazány (řeší PlayerStats)

    [Header("Animation")]
    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // ❌ Start smazán (řeší PlayerStats)

    void Update()
    {
        if (!isDashing)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement.Normalize();
        }

        // 🔥 Kontrola staminy přes PlayerStats
        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector2.zero && !isDashing)
        {
            // Zeptáme se Banky, jestli máme dost energie
            if (PlayerStats.Instance.HasStamina(dashEnergyCost))
            {
                StartCoroutine(Dash());
            }
        }

        // ❌ Regenerace smazána (řeší PlayerStats)

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        // 🔥 Utratíme staminu z Banky
        PlayerStats.Instance.UseStamina(dashEnergyCost);

        rb.velocity = movement * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.velocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
}