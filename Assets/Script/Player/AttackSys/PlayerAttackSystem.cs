using UnityEngine;
// using UnityEngine.EventSystems; // Už není potřeba, smazali jsme tu kontrolu

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
        // 1. KONTROLA UIMANAGERA (Je otevřené nějaké velké okno?)
        // Pokud UIManager existuje a některé z jeho menu je otevřené, zakážeme útok.
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.isGameMenuOpen ||
                UIManager.Instance.isPaused ||
                UIManager.Instance.isCrystalUIOpen ||
                UIManager.Instance.isTutorialOpen ||
                UIManager.Instance.isDead)
            {
                return; // Jsme v menu -> neútočit
            }
        }

        // ZDE BYLA KONTROLA MYŠI (EventSystem) - SMAZÁNO.
        // Teď můžeš útočit i s myší na HP baru.

        // --- Samotný útok ---
        if (currentAttack == null) return;
        if (Time.time < nextAttackTime) return;

        if (Input.GetMouseButtonDown(0))
        {
            float multiplier = stats != null ? stats.damageMultiplier : 1f;
            currentAttack.PerformAttack(transform, cam, enemyLayers, multiplier);
            nextAttackTime = Time.time + 1f / currentAttack.attackRate;
        }
    }
}