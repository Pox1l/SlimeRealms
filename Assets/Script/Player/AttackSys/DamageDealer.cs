using UnityEngine;
using System.Collections.Generic; // Přidáno pro List

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float lifetime = 0.3f;
    public LayerMask enemyLayers;
    public bool destroyOnHit = true; // PRO PLOŠNÝ ÚTOK (AOE) TOTO VYPNOUT!

    // 🔒 ZMĚNA: Místo boolu (jeden zásah) používáme seznam trefených objektů
    // private bool hasHit = false; 
    private List<GameObject> hitObjects = new List<GameObject>();

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ZMĚNA: Kontrolujeme, jestli už jsme TENTO KONKRÉTNÍ objekt trefili
        // Pokud je v seznamu, ignorujeme ho (aby nedostal dmg 2x), ale ostatní projdou dál
        if (hitObjects.Contains(collision.gameObject)) return;

        // Kontrola Layeru (Ujisti se, že Boss je na vrstvě, která je tu zaškrtnutá!)
        if ((enemyLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        // --- ZMĚNA ZAČÍNÁ ZDE ---

        // Přidáme objekt do seznamu, abychom ho už znovu netrefili
        hitObjects.Add(collision.gameObject);

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
        // hasHit = true; // ZMĚNA: Smazáno, řešíme to seznamem nahoře

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
            // POZOR: Pokud je toto zapnuté, zničí se to u prvního nepřítele
            // a ostatní už to nestihne trefit!
            Destroy(gameObject);
        }
    }
}