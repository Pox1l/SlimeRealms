using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

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
            if (drop != null) drop.DropLoot();
            OnDeath?.Invoke(); 
            // OnDeath.Invoke(); 
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