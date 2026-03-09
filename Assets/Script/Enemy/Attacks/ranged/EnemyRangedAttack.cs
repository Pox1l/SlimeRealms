using UnityEngine;
using UnityEngine.AI;
using FMODUnity; // <--- Důležité: Přidat knihovnu

public class EnemyRangedAttack : MonoBehaviour
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

    [Header("Audio")] // <--- NOVÁ SEKCE
    [Tooltip("Zvuk výstřelu. Pokud je prázdný, použije se Default Ranged z AudioManageru.")]
    public EventReference shootSound;

    [Header("2D Settings & Clearance")]
    public LayerMask whatIsTarget;
    public float clearanceDuration = 0.4f;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    // Clearance variables
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

        // --- 1. KONTROLA VIDITELNOSTI ---
        bool hasLineOfSight = CheckLineOfSight();

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

        // --- 2. OTOČENÍ ZBRANĚ ---
        if (isAttacking || hasLineOfSight)
        {
            RotateGunToPlayer();
            if (aimLine != null && aimLine.enabled) UpdateAimLinePosition();
        }

        // --- 3. ROZHODOVÁNÍ O POHYBU ---
        // OPRAVA: Výchozí stav je false, aby nechodil k hráči přes celou mapu
        bool shouldMove = false;

        if (isAttacking)
        {
            shouldMove = false;
        }
        // OPRAVA: Řešíme pohyb k hráči jen pokud je v dosahu pro útok
        else if (distanceToPlayer <= attackRange)
        {
            if (hasLineOfSight && !isClearingCorner)
            {
                if (distanceToPlayer > keepDistance)
                {
                    shouldMove = true; // OPRAVA: Jde blíž, pokud je dál než keepDistance
                }
                else
                {
                    shouldMove = false; // OPRAVA: Stojí, pokud je blízko
                }

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartAttackSequence();
                }
            }
            else
            {
                shouldMove = true; // Zkouší najít hráče, pokud nemá Line of Sight, ale je v range
            }
        }

        // --- 4. APLIKACE POHYBU ---
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

    bool CheckLineOfSight()
    {
        if (playerTransform == null) return false;
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, playerTransform.position);
        float checkDist = Mathf.Min(distance, attackRange);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, checkDist, whatIsTarget);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player")) return true;
        }
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

    void StartAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (animator != null) animator.SetTrigger("Attack");

        // Poznámka: Pokud bys chtěl zvuk nabíjení laseru, dal bys ho sem.

        if (aimLine != null)
        {
            aimLine.enabled = true;
            RotateGunToPlayer();
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

    // 🔥 TADY SE DĚJE VÝSTŘEL
    public void Shoot()
    {
        if (aimLine != null) aimLine.enabled = false;

        // --- AUDIO: ZVUK VÝSTŘELU ---
        if (AudioManager.instance != null)
        {
            // Voláme speciální funkci pro Ranged útoky
            // U střelby je lepší dát pozici firePointu (hlaveň):
            AudioManager.instance.PlayRangedAttack(shootSound, firePoint.position);
        }
        // -----------------------------

        if (projectilePrefab == null || firePoint == null || fixedPoint == null) return;
        RotateGunToPlayer();
        Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}