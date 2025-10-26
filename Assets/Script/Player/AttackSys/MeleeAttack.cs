using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    public Vector2 boxSize = new Vector2(1.6f, 0.8f);
    public float boxDistance = 1.0f;
    public GameObject slashPrefab;
    public float slashDuration = 0.2f;

    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers)
    {
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 playerPos = attacker.position;
        Vector2 aimDir = (Vector2)(mouseWorld - (Vector3)playerPos);
        if (aimDir.sqrMagnitude < 0.0001f) aimDir = Vector2.right;
        aimDir.Normalize();

        float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Vector2 center = playerPos + aimDir * boxDistance;

        // Spawn efektu
        if (slashPrefab)
        {
            GameObject slash = GameObject.Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg - 90));
            GameObject.Destroy(slash, slashDuration);
        }

        // Damage check
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
