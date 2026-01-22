using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public float lifeTime = 2f;

    // Tady si v Inspectoru nastavíš, co je pro kulku "zeï" (napø. Layer 'Ground' nebo 'Default')
    public LayerMask whatIsObstacle;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Trefil jsem hráèe?
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage, transform);
            }
            Destroy(gameObject);
            return; // Ukonèíme funkci, abychom nekontrolovali dál
        }

        // 2. Trefil jsem zeï? (Zkontroluje, jestli objekt, do kterého jsme vrazili, je v LayerMask)
        if (((1 << other.gameObject.layer) & whatIsObstacle) != 0)
        {
            Destroy(gameObject);
        }

        // Pokud to trefilo Enemy nebo jinou kulku, díky nastavení Physics Matrix se nestane nic a letí dál.
    }
}