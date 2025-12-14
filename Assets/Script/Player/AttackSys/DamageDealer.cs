using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float lifetime = 0.3f;
    public LayerMask enemyLayers;
    public bool destroyOnHit = true;

    // 🔒 Pojistka proti dvojitému zásahu
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Pokud už jsme někoho trefili, okamžitě skonči
        if (hasHit) return;

        // Kontrola Layeru
        if ((enemyLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        if (collision.TryGetComponent(out EnemyHealth enemy))
        {
            // 2. Označíme, že zásah proběhl
            hasHit = true;

            enemy.TakeDamage(damage);
            Debug.Log($"Hit {collision.name}, dealt {damage} dmg");

            if (destroyOnHit)
                Destroy(gameObject);
        }
    }
}