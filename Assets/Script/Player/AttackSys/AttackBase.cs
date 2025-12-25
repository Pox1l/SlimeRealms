using UnityEngine;

public abstract class AttackBase : ScriptableObject
{
    public Sprite icon;
    public string attackName = "New Attack";
    public float attackRate = 1f;
    public int baseDamage = 10; // Přejmenováno na baseDamage

    // ⚡ PŘIDÁNO: float damageMultiplier
    public abstract void PerformAttack(Transform attacker, Camera cam, LayerMask enemyLayers, float damageMultiplier);
}