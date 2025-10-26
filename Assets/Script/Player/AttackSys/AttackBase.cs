using UnityEngine;

public abstract class AttackBase : ScriptableObject
{
    public string attackName = "New Attack";
    public float attackRate = 1f;
    public int damage = 10;

    public abstract void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers);
}
