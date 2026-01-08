using UnityEngine;

public class EnemyMeleeHitbox : MonoBehaviour
{
    public int damage = 20;
    public float lifetime = 0.5f;

    // 🔥 NOVÉ: Pojistka, aby to dalo DMG jen jednou
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Pokud už jsme někoho trefili, nic nedělej
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage, transform);

                // 🔥 NOVÉ: Nastavíme, že útok proběhl
                hasHit = true;

                // Volitelné: Můžeš ho zničit hned, aby dál nepřekážel
                // Destroy(gameObject);
            }
        }
    }
}