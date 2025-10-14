using UnityEngine;
using System;

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

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 15f;

    [Header("Animation")]
    public Animator animator; // pøidáno

    // Event pro UI
    public event Action<float, float> OnEnergyChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentEnergy = maxEnergy;
    }

    void Start()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    void Update()
    {
        if (!isDashing)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement.Normalize();
        }

        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector2.zero && currentEnergy >= dashEnergyCost)
        {
            StartCoroutine(Dash());
        }

        // regenerace energie
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;

        currentEnergy -= dashEnergyCost;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

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
