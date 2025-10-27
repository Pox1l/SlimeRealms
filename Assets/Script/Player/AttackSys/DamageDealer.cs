using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;               // kolik dmg dává
    public float lifetime = 0.3f;         // jak dlouho žije prefab (např. slash efekt)
    public LayerMask enemyLayers;         // jaké vrstvy zraňuje
    public bool destroyOnHit = true;      // zda se má zničit po zásahu (u střely ano)

    void Start()
    {
        // automaticky se zničí po čase (např. vizuální slash)
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🧠 Zkontroluj, jestli je objekt v layers určených pro enemy
        if ((enemyLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        if (collision.TryGetComponent(out EnemyHealth enemy))
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Hit {collision.name}, dealt {damage} dmg");

            if (destroyOnHit)
                Destroy(gameObject);
        }
    }
}
