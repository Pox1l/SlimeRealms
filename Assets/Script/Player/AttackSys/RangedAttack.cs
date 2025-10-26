using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Ranged Attack")]
public class RangedAttack : AttackBase
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    public override void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers)
    {
        if (projectilePrefab == null) return;
        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        // vypočítáme světovou pozici kurzoru
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // směr je vždy z hráče k myši
        Vector2 dir = ((Vector2)(mouseWorld - attacker.position)).normalized;

        // spawn z range pointu (aby nevystřeloval zevnitř hráče)
        PlayerAttackSystem atkSystem = attacker.GetComponent<PlayerAttackSystem>();
        Transform spawn = atkSystem != null ? atkSystem.rangePoint : attacker;

        // vytvoření střely
        GameObject proj = GameObject.Instantiate(projectilePrefab, spawn.position, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        // natočení střely (volitelné – vizuální)
        proj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }




}
