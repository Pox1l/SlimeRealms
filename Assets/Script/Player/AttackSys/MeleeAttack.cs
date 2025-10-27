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

    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers)
    {
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        // 🟢 Reference na attack systém
        PlayerAttackSystem attackSystem = attacker.GetComponent<PlayerAttackSystem>();
        Transform meleePoint = attackSystem != null ? attackSystem.meleePoint : attacker;

        // 🎯 Myš na světové souřadnice
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 🧭 Směr od meleePointu k myši
        Vector2 meleePos = meleePoint.position;
        Vector2 aimDir = (mouseWorld - (Vector3)meleePos).normalized;

        // Úhel a střed hitboxu
        float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Vector2 center = meleePos + aimDir * boxDistance;

        // ✨ Slash efekt
        if (slashPrefab)
        {
            var slash = GameObject.Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg - 90));
            GameObject.Destroy(slash, slashDuration);
        }

        // 💥 Damage check
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Hit enemy {hit.name}, damage {damage}");
            }
        }
    }
}
