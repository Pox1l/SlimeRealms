using UnityEngine;
using System;
using FMODUnity;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 500;
    public int currentHealth;

    [Header("Audio")]
    [Tooltip("Pokud necháš prázdné, zahraje se Default Hit z Manageru")]
    public EventReference hitSound;

    [Header("VFX")]
    [Tooltip("Vlož prefab efektu, který se spawnne při smrti")]
    public GameObject deathEffectPrefab;

    [Header("VFX Rotation")]
    [Tooltip("Rotace (Eulerovy úhly X, Y, Z) pro efekt při spawnu. Nastav X na 90.")]
    public Vector3 deathEffectSpawnRotation = new Vector3(90f, 0f, 0f);

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    private BossDrop bossDrop;
    private EnemyController controller;
    private DamageFlash damageFlash;
    private EnemyKnockback knockback;
    private BossEncounter bossEncounter;

    void Awake()
    {
        bossDrop = GetComponent<BossDrop>();
        controller = GetComponent<EnemyController>();
        damageFlash = GetComponent<DamageFlash>();
        knockback = GetComponent<EnemyKnockback>();
    }

    void Start()
    {
        bossEncounter = FindObjectOfType<BossEncounter>();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHP(currentHealth, maxHealth);
        }
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayHitSound(hitSound, transform.position);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHP(currentHealth, maxHealth);
        }

        if (damageFlash != null) damageFlash.Flash();
        if (knockback != null) knockback.PlayKnockback();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (controller != null) controller.OnHitAggro();
        }
    }

    void Die()
    {
        Debug.Log("💀 Boss defeated!");

        // Spawnnutí efektu na pozici bosse se specifickou rotací
        if (deathEffectPrefab != null)
        {
            Quaternion spawnRotation = Quaternion.Euler(deathEffectSpawnRotation);
            Instantiate(deathEffectPrefab, transform.position, spawnRotation);
        }

        if (bossDrop != null) bossDrop.DropLoot();

        if (bossEncounter != null)
        {
            bossEncounter.SetBossDefeated();
        }
        else
        {
            bossEncounter = FindObjectOfType<BossEncounter>();
            if (bossEncounter != null) bossEncounter.SetBossDefeated();
        }

        OnDeath?.Invoke();
    }
}