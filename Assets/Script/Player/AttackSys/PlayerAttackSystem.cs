using System.Collections;
using System.Collections.Generic;
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

    private float nextAttackTime;

    void Update()
    {
        if (currentAttack == null) return;
        if (Time.time < nextAttackTime) return;

        if (Input.GetMouseButtonDown(0))
        {
            currentAttack.PerformAttack(transform, cam, enemyLayers);
            nextAttackTime = Time.time + 1f / currentAttack.attackRate;
        }
    }
}
