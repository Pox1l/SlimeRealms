using UnityEngine;
using System.Collections.Generic;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float lifetime = 0.3f;
    public LayerMask enemyLayers;
    public LayerMask obstacleLayers; // NOVÉ: Vrstvy, které projektil zastaví (např. Ground/Wall)
    public bool destroyOnHit = true;

    private List<GameObject> hitObjects = new List<GameObject>();

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. KONTROLA PŘEKÁŽEK (Zdi, podlaha)
        // Pokud narazíme na layer v obstacleLayers, objekt se zničí bez ohledu na damage
        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // 2. KONTROLA DUPLICIT
        if (hitObjects.Contains(collision.gameObject)) return;

        // 3. KONTROLA NEPŘÁTELSKÝCH LAYERŮ
        if ((enemyLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        hitObjects.Add(collision.gameObject);

        if (collision.TryGetComponent(out EnemyHealth enemy))
        {
            HitTarget(enemy);
        }
        else if (collision.TryGetComponent(out BossHealth boss))
        {
            HitTarget(boss);
        }
    }

    private void HitTarget(Component target)
    {
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