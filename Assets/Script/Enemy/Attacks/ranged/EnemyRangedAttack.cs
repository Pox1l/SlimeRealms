using UnityEngine;
using UnityEngine.AI;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;

    // 🔥 DŮLEŽITÉ: Rodič FirePointu (střed otáčení)
    public Transform fixedPoint;
    // Ústí hlavně (dítě fixedPointu)
    public Transform firePoint;

    public Animator animator;
    public LineRenderer aimLine;

    [Header("Combat Ranges")]
    public float attackRange = 7f;
    public float stoppingDistance = 4f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f;
    public float aimLineLength = 10f;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (aimLine == null) aimLine = GetComponent<LineRenderer>();
        if (aimLine != null) aimLine.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Pojistka proti chybám při knockbacku
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        // 🔥 1. Pokud útočíme, otáčíme zbraní za hráčem
        if (isAttacking)
        {
            RotateGunToPlayer();

            // Aktualizace Aim Line, aby vycházela ze správného místa
            if (aimLine != null && aimLine.enabled)
            {
                UpdateAimLinePosition();
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // --- 2. LOGIKA POHYBU ---
        if (!isAttacking)
        {
            if (distanceToPlayer <= stoppingDistance)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                FacePlayer();
            }
            else
            {
                agent.isStopped = false;
            }
        }

        // --- 3. LOGIKA ÚTOKU ---
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttackSequence();
        }
    }

    // 🔥 Funkce pro otáčení fixedPointu
    void RotateGunToPlayer()
    {
        if (fixedPoint == null || playerTransform == null) return;

        Vector2 dir = playerTransform.position - fixedPoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Aplikujeme rotaci na fixedPoint
        fixedPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateAimLinePosition()
    {
        aimLine.SetPosition(0, firePoint.position);

        Vector2 dirToPlayer = (playerTransform.position - firePoint.position).normalized;
        Vector3 endPosition = firePoint.position + (Vector3)(dirToPlayer * aimLineLength);
        aimLine.SetPosition(1, endPosition);
    }

    void StartAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack");

        if (aimLine != null)
        {
            aimLine.enabled = true;
            RotateGunToPlayer(); // Prvotní srovnání
            UpdateAimLinePosition();
        }

        Invoke("FinishAttack", 1.5f);
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (aimLine != null) aimLine.enabled = false;
        CancelInvoke("FinishAttack");
    }

    // 🔥 Tady je ta změna pro rotaci projektilu
    public void Shoot()
    {
        if (aimLine != null) aimLine.enabled = false;

        if (projectilePrefab == null || firePoint == null || fixedPoint == null) return;

        // Pro jistotu naposledy srovnáme rotaci před výstřelem, aby to bylo přesné
        RotateGunToPlayer();

        // 🔥 ZMĚNA: Použijeme fixedPoint.rotation.
        // Tím zajistíme, že projektil bude natočený stejně jako zbraň.
        Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
    }

    void FacePlayer()
    {
        // Otáčíme jen tělo enemyho (ne zbraň, tu řeší RotateGunToPlayer)
        if (playerTransform.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}