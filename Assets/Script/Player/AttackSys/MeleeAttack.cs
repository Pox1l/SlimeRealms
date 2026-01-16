using UnityEngine;
using System.Collections.Generic;

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

        int finalDamage = Mathf.CeilToInt(baseDamage * damageMultiplier);

        // --- Vizuální efekt ---
        if (slashPrefab)
        {
            var slash = Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg - 90));

            // Zničíme DamageDealer na efektu
            var visualDealer = slash.GetComponent<DamageDealer>();
            if (visualDealer != null) Destroy(visualDealer);

            Destroy(slash, slashDuration);
        }

        // --- APLIKACE DAMAGE (S opravou dvojitého zásahu) ---
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);

        // Seznam už trefených skriptů
        List<MonoBehaviour> alreadyHitTargets = new List<MonoBehaviour>();

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == attacker.gameObject) continue;

            // 1. Zkusíme EnemyHealth
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                if (alreadyHitTargets.Contains(enemy)) continue;

                enemy.TakeDamage(finalDamage);
                alreadyHitTargets.Add(enemy);

                // 🛠️ OPRAVA ZDE: Smazal jsem "(HP: {enemy.currentHealth})", 
                // protože currentHealth je private a nešlo to přečíst.
                Debug.Log($"Melee hit: {enemy.name}");
            }
            // 2. Zkusíme BossHealth
            else if (hit.TryGetComponent(out BossHealth boss))
            {
                if (alreadyHitTargets.Contains(boss)) continue;

                boss.TakeDamage(finalDamage);
                alreadyHitTargets.Add(boss);
                Debug.Log($"Melee hit BOSS: {boss.name}");
            }
        }
    }
}