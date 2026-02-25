using UnityEngine;
using FMODUnity; // 🔥 1. Nutné přidat knihovnu

[CreateAssetMenu(menuName = "Attacks/Ranged Attack")]
public class RangedAttack : AttackBase
{
    [Header("Visual Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    [Header("Audio (FMOD)")]
    public EventReference shootSound; // 🔥 2. Zvuk výstřelu (Bow release, Gunshot...)

    // ⚡ UPRAVENO: Přijímáme damageMultiplier
    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers, float damageMultiplier)
    {
        if (projectilePrefab == null) return;
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        // 🔥 3. Přehrát zvuk výstřelu (Hned na začátku)
        if (!shootSound.IsNull)
        {
            RuntimeManager.PlayOneShot(shootSound, attacker.position);
        }

        // Pozice myši
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // Směr
        Vector2 dir = ((Vector2)(mouseWorld - attacker.position)).normalized;

        // Spawn point
        PlayerAttackSystem atkSystem = attacker.GetComponent<PlayerAttackSystem>();
        Transform spawn = atkSystem != null ? atkSystem.rangePoint : attacker;

        // Vytvoření střely
        GameObject proj = Instantiate(projectilePrefab, spawn.position, Quaternion.identity);

        // Pohyb střely
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        // Natočení střely
        proj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        // 🔥 APLIKACE DAMAGE ZE SKILL TREE 🔥
        // Najdeme na projektilu skript DamageDealer a přepíšeme mu damage
        DamageDealer dealer = proj.GetComponent<DamageDealer>();
        if (dealer != null)
        {
            // 🔥 OPRAVA: ZMĚNA Z * NA +
            int bonusDamage = Mathf.RoundToInt(damageMultiplier);
            int finalDamage = baseDamage + bonusDamage;

            dealer.damage = finalDamage;
            dealer.enemyLayers = enemyLayers;
        }
        else
        {
            Debug.LogWarning("Projektil nemá komponentu DamageDealer!");
        }
    }
}