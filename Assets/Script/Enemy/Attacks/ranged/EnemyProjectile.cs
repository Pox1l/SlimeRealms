using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;

    void Start()
    {
        // Znièení po 5 vteøinách, aby nezatìžoval hru, pokud nic netrefí
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // DÙLEŽITÉ: Letí "doprava" (Vector2.right) v lokálním prostoru.
        // Protože jsme ho v pøedchozím scriptu otoèili èumákem k hráèi,
        // jeho "doprava" je nyní smìr k hráèi.
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Místo 'transform' mùžeš poslat null, pokud nepotøebuješ vìdìt, kdo dmg udìlil
                stats.TakeDamage(damage, transform);
            }
            Destroy(gameObject); // Znièit projektil po zásahu
        }
        
    }
}