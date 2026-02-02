using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RangedCrystalAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;
    public LineRenderer aimLine;

    [Header("Combat Ranges")]
    public float attackRange = 7f;
    public float keepDistance = 3f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f;
    public float aimLineLength = 10f;

    [Header("Burst Settings")]
    public int shotCount = 4;
    public float timeBetweenShots = 1f; // Čas mezi jednotlivými náboji
    public float recoveryTime = 1f;     // 🔥 NOVÉ: Pauza po dokončení celé série

    [Header("2D Settings & Clearance")]
    public LayerMask whatIsTarget;
    public float clearanceDuration = 0.4f;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    // Logika nadbíhání
    private float clearanceTimer = 0f;
    private bool wasBlocked = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.autoBraking = false;
        }

        if (aimLine == null) aimLine = GetComponent<LineRenderer>();
        if (aimLine != null) aimLine.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool hasLineOfSight = CheckLineOfSight();

        // --- 1. LOGIKA NADBĚHNUTÍ ---
        if (!hasLineOfSight)
        {
            wasBlocked = true;
        }
        else if (hasLineOfSight && wasBlocked)
        {
            clearanceTimer = Time.time + clearanceDuration;
            wasBlocked = false;
        }

        bool isClearingCorner = Time.time < clearanceTimer;

        // --- 2. OTOČENÍ ---
        if (isAttacking || hasLineOfSight)
        {
            RotateGunToPlayer();
            if (aimLine != null && aimLine.enabled) UpdateAimLinePosition();
        }

        // --- 3. POHYB ---
        bool shouldMove = true;

        if (isAttacking)
        {
            shouldMove = false; // Během útoku i během "recovery" pauzy stojí
        }
        else if (hasLineOfSight && distanceToPlayer <= attackRange && !isClearingCorner)
        {
            if (distanceToPlayer > keepDistance) shouldMove = false;
            else shouldMove = false;

            // Kontrola cooldownu
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttackSequence();
            }
        }

        if (shouldMove)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    void StartAttackSequence()
    {
        if (isAttacking) return;

        isAttacking = true;
        // Nastavíme čas posledního útoku. 
        // Pozor: Pokud je cooldown kratší než trvání střelby + recovery, bude útočit hned znova.
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack");

        StartCoroutine(BurstFireRoutine());
    }

    IEnumerator BurstFireRoutine()
    {
        for (int i = 0; i < shotCount; i++)
        {
            if (this == null || !gameObject.activeInHierarchy) yield break;
            if (playerTransform == null) break;

            // Kontrola úniku hráče (zachována z minula)
            float currentDist = Vector2.Distance(transform.position, playerTransform.position);
            bool currentSight = CheckLineOfSight();

            if (currentDist > attackRange || !currentSight)
            {
                FinishAttack(); // Okamžitě přeruší a jde běhat
                yield break;
            }

            // Zapnutí AimLine
            if (aimLine != null)
            {
                aimLine.enabled = true;
                UpdateAimLinePosition();
            }

            Shoot();

            // Čekání mezi střelami
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // 🔥 NOVÉ: Čekání po dokončení celé série (recovery time)
        // Enemy stále stojí na místě (protože isAttacking je stále true)
        yield return new WaitForSeconds(recoveryTime);

        FinishAttack();
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (aimLine != null) aimLine.enabled = false;

        // Volitelné: Reset cooldownu až po dokončení střelby, 
        // pokud chceš, aby cooldown 2s běžel až po té "recovery" pauze:
        lastAttackTime = Time.time;
    }

    public void Shoot()
    {
        if (aimLine != null) aimLine.enabled = false;

        if (projectilePrefab != null && firePoint != null && fixedPoint != null)
        {
            RotateGunToPlayer();
            Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
        }
    }

    bool CheckLineOfSight()
    {
        if (playerTransform == null) return false;
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, playerTransform.position);
        float checkDist = Mathf.Min(distance, attackRange);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, checkDist, whatIsTarget);

        if (hit.collider != null && hit.collider.CompareTag("Player")) return true;
        return false;
    }

    void RotateGunToPlayer()
    {
        if (fixedPoint == null || playerTransform == null) return;
        Vector2 dir = playerTransform.position - fixedPoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fixedPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateAimLinePosition()
    {
        aimLine.SetPosition(0, firePoint.position);
        Vector2 dirToPlayer = (playerTransform.position - firePoint.position).normalized;
        Vector3 endPosition = firePoint.position + (Vector3)(dirToPlayer * aimLineLength);
        aimLine.SetPosition(1, endPosition);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}