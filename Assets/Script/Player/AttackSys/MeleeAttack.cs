using UnityEngine;
using System.Collections.Generic;
using FMODUnity; // 🔥 Nutné pro FMOD

[CreateAssetMenu(menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    [Header("Hitbox Settings")]
    public Vector2 boxSize = new Vector2(1.6f, 0.8f);
    public float boxDistance = 1.0f;

    [Header("Visual Effect")]
    public GameObject slashPrefab;
    public float slashDuration = 0.2f;

    [Header("Audio (FMOD)")]
    public EventReference swingSound; // Zvuk máchnutí (vzduch)
    public EventReference hitSound;   // Zvuk zásahu (maso/kov)

    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers, float damageMultiplier)
    {
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        // 🔥 1. Přehrát zvuk máchnutí (vždy)
        if (!swingSound.IsNull)
        {
            RuntimeManager.PlayOneShot(swingSound, attacker.position);
        }

        PlayerAttackSystem attackSystem = attacker.GetComponent<PlayerAttackSystem>();
        Transform meleePoint = attackSystem != null ? attackSystem.meleePoint : attacker;

        // --- Výpočet pozice ---
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 meleePos = meleePoint.position;
        Vector2 aimDir = (mouseWorld - (Vector3)meleePos).normalized;
        float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        Vector2 center = meleePos + aimDir * boxDistance;

        // -----------------------------------------------------------------------
        // 🔥 OPRAVA: PŘECHOD NA FLAT DAMAGE (SČÍTÁNÍ)
        // Místo násobení (base * multiplier) nyní přičítáme bonus (base + bonus).
        // damageMultiplier teď reprezentuje číslo, které se přičte (např. +2, +5).
        // -----------------------------------------------------------------------
        int bonusDamage = Mathf.RoundToInt(damageMultiplier);
        int finalDamage = baseDamage + bonusDamage;

        // Debug pro kontrolu (pokud bys chtěl vidět čísla v konzoli, odkomentuj)
        // Debug.Log($"Melee Attack: Base {baseDamage} + Bonus {bonusDamage} = {finalDamage}");

        // --- Vizuální efekt ---
        if (slashPrefab)
        {
            var slash = Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg - 90));

            var visualDealer = slash.GetComponent<DamageDealer>();
            if (visualDealer != null) Destroy(visualDealer);

            Destroy(slash, slashDuration);
        }

        // --- APLIKACE DAMAGE ---
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);

        List<MonoBehaviour> alreadyHitTargets = new List<MonoBehaviour>();
        bool didHitSomething = false; // Kontrola pro zvuk zásahu

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == attacker.gameObject) continue;

            bool hitSuccess = false;

            // 1. Zkusíme EnemyHealth
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                if (alreadyHitTargets.Contains(enemy)) continue;

                enemy.TakeDamage(finalDamage);
                alreadyHitTargets.Add(enemy);
                hitSuccess = true;
                // Debug.Log($"Melee hit: {enemy.name}");
            }
            // 2. Zkusíme BossHealth
            else if (hit.TryGetComponent(out BossHealth boss))
            {
                if (alreadyHitTargets.Contains(boss)) continue;

                boss.TakeDamage(finalDamage);
                alreadyHitTargets.Add(boss);
                hitSuccess = true;
                // Debug.Log($"Melee hit BOSS: {boss.name}");
            }

            if (hitSuccess) didHitSomething = true;
        }

        // 🔥 2. Přehrát zvuk zásahu (pokud jsme něco trefili)
        if (didHitSomething && !hitSound.IsNull)
        {
            // Přehráváme zvuk na místě zásahu (center)
            RuntimeManager.PlayOneShot(hitSound, center);
        }
    }
}