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

        // Kontrola Layeru (Ujisti se, že Boss je na vrstvě, která je tu zaškrtnutá!)
        if ((enemyLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        // --- ZMĚNA ZAČÍNÁ ZDE ---

        // 2. Nejprve zkusíme najít běžného nepřítele (EnemyHealth)
        if (collision.TryGetComponent(out EnemyHealth enemy))
        {
            HitTarget(enemy); // Voláme pomocnou metodu dole
        }
        // 3. Pokud to není běžný enemy, zkusíme najít BOSSE (BossHealth)
        else if (collision.TryGetComponent(out BossHealth boss))
        {
            HitTarget(boss); // Voláme pomocnou metodu dole
        }

        // --- ZMĚNA KONČÍ ZDE ---
    }

    // Pomocná metoda, aby se kód neopakoval
    private void HitTarget(Component target)
    {
        hasHit = true;

        // Rozlišení, komu dáváme damage
        if (target is EnemyHealth e)
        {
            e.TakeDamage(damage);
        }
        else if (target is BossHealth b)
        {
            b.TakeDamage(damage);
        }

        Debug.Log($"Hit {target.name}, dealt {damage} dmg");

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}