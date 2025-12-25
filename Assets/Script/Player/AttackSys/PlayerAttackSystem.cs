using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("General Settings")]
    public LayerMask enemyLayers;
    public Camera cam;
    public Transform meleePoint;
    public Transform rangePoint;

    [Header("Active Attack")]
    public AttackBase currentAttack;
    public float nextAttackTime;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (currentAttack == null) return;
        if (Time.time < nextAttackTime) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Získáme bonus (nebo 1, pokud chybí stats)
            float multiplier = stats != null ? stats.damageMultiplier : 1f;

            // ⚡ ODESLÁNÍ MULTIPLIERU DO ÚTOKU
            currentAttack.PerformAttack(transform, cam, enemyLayers, multiplier);

            nextAttackTime = Time.time + 1f / currentAttack.attackRate;
        }
    }
}