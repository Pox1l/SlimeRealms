using UnityEngine;
using System;
using FMODUnity;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Audio")]
    [Tooltip("Pokud necháš prázdné, zahraje se Default Hit z Manageru")]
    public EventReference hitSound;

    [Header("VFX")]
    [Tooltip("Vlož prefab efektu, který se spawnne při smrti")]
    public GameObject deathEffectPrefab;

    [Header("VFX Rotation")] // Nové záhlaví pro přehlednost
    [Tooltip("Rotace (Eulerovy úhly X, Y, Z) pro efekt při spawnu. Nastav X na 90.")]
    public Vector3 deathEffectSpawnRotation = new Vector3(90f, 0f, 0f); // Výchozí hodnota je 90 na X

    public event Action OnDeath;

    private EnemyDrop drop;
    private EnemyController controller;
    private DamageFlash damageFlash;
    private EnemyKnockback knockback;

    void Awake()
    {
        drop = GetComponent<EnemyDrop>();
        controller = GetComponent<EnemyController>();
        damageFlash = GetComponent<DamageFlash>();
        knockback = GetComponent<EnemyKnockback>();
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayHitSound(hitSound, transform.position);
        }

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (knockback != null)
        {
            knockback.PlayKnockback();
        }

        if (currentHealth <= 0)
        {
            // Spawnnutí efektu na pozici nepřítele se specifickou rotací
            if (deathEffectPrefab != null)
            {
                // Převod Eulerových úhlů na Quaternion
                Quaternion spawnRotation = Quaternion.Euler(deathEffectSpawnRotation);
                Instantiate(deathEffectPrefab, transform.position, spawnRotation);
            }

            if (drop != null) drop.DropLoot();
            OnDeath?.Invoke(); // Tady se vyšle signál o smrti
        }
        else
        {
            if (controller != null)
            {
                controller.OnHitAggro();
            }
        }
    }
}