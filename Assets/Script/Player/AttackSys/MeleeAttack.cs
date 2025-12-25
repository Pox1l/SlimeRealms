using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    [Header("Hitbox Settings")]
    public Vector2 boxSize = new Vector2(1.6f, 0.8f);
    public float boxDistance = 1.0f;

    [Header("Visual Effect")]
    public GameObject slashPrefab;
    public float slashDuration = 0.2f;

    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers, float damageMultiplier)
    {
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        PlayerAttackSystem attackSystem = attacker.GetComponent<PlayerAttackSystem>();
        Transform meleePoint = attackSystem != null ? attackSystem.meleePoint : attacker;

        // --- Výpočet pozice ---
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 meleePos = meleePoint.position;
        Vector2 aimDir = (mouseWorld - (Vector3)meleePos).normalized;
        float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Vector2 center = meleePos + aimDir * boxDistance;

        // --- VÝPOČET SKUTEČNÉHO DAMAGE ---
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        // --- Vytvoření efektu ---
        if (slashPrefab)
        {
            var slash = Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg - 90));

            // 🔥 UPDATE VIZUÁLU: Najdeme DamageDealer na efektu a přepíšeme mu damage
            // (Jen aby to v Inspectoru vypadalo správně, nebo pro případ, že používáš kolize)
            var visualDealer = slash.GetComponent<DamageDealer>();
            if (visualDealer != null)
            {
                visualDealer.damage = finalDamage;
                visualDealer.enemyLayers = enemyLayers;

                // POZOR: Pokud teď má efekt zapnutý Collider, dá damage taky! 
                // Pokud chceš damage jen přes OverlapBox (dole), vypni tady collider:
                // var col = slash.GetComponent<Collider2D>();
                // if (col) col.enabled = false; 
            }

            Destroy(slash, slashDuration);
        }

        // --- APLIKACE DAMAGE (Přes OverlapBox - okamžitý zásah) ---
        // Toto je ten "hlavní" útok
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            // Zkontrolujeme, jestli jsme netrefili sami sebe nebo vizuální efekt
            if (hit.gameObject == attacker.gameObject) continue;

            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(finalDamage);
            }
            else if (hit.TryGetComponent(out BossHealth boss))
            {
                boss.TakeDamage(finalDamage);
            }
        }
    }
}