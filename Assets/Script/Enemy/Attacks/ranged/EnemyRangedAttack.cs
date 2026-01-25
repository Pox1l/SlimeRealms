using UnityEngine;
using UnityEngine.AI;

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
    // Stopping distance v inspektoru na NavMeshAgentovi dej na 0!
    // Tuhle proměnnou používáme jen pro logiku, kdyby byl moc blízko.
    public float keepDistance = 3f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f;
    public float aimLineLength = 10f;

    [Header("2D Settings & Clearance")]
    public LayerMask whatIsTarget;
    // 🔥 NOVÉ: Jak dlouho (v sekundách) má ještě jít poté, co uvidí hráče, aby si "nadběhl" roh.
    public float clearanceDuration = 0.4f;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    // 🔥 NOVÉ: Proměnné pro logiku "nadběhnutí"
    private float clearanceTimer = 0f;
    private bool wasBlocked = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // FIX PRO 2D
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            // Důležité: Vypneme automatické brždění, aby reagoval svižněji
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

        // 🔥 LOGIKA "NADBĚHNUTÍ" (CLEARANCE)
        if (!hasLineOfSight)
        {
            // Pokud ho nevidíme, poznačíme si, že jsme zablokovaní
            wasBlocked = true;
        }
        else if (hasLineOfSight && wasBlocked)
        {
            // PRÁVĚ JSME VYKOUKLI ZPOZA ROHU (Vidíme ho, ale před chvílí jsme ho neviděli)
            // Nastavíme časovač, do kdy se musíme ještě hýbat
            clearanceTimer = Time.time + clearanceDuration;
            // Resetujeme flag, už nejsme zablokovaní
            wasBlocked = false;
        }

        // Zjistíme, jestli ještě běží čas na nadběhnutí
        bool isClearingCorner = Time.time < clearanceTimer;


        // --- 2. OTOČENÍ ZBRANĚ ---
        if (isAttacking || hasLineOfSight)
        {
            RotateGunToPlayer();
            if (aimLine != null && aimLine.enabled) UpdateAimLinePosition();
        }

        // --- 3. ROZHODOVÁNÍ O POHYBU ---
        bool shouldMove = true;

        if (isAttacking)
        {
            shouldMove = false;
        }
        // 🔥 UPRAVENÁ PODMÍNKA ZASTAVENÍ:
        // Zastaví jen pokud: Vidí hráče A je v dostřelu A UŽ DOBĚHL clearance časovač.
        else if (hasLineOfSight && distanceToPlayer <= attackRange && !isClearingCorner)
        {
            // Pokud je moc blízko, ať radši couve nebo stojí (volitelné, teď ho necháme stát)
            if (distanceToPlayer > keepDistance)
            {
                shouldMove = false; // STŮJ A STŘÍLEJ
            }
            else
            {
                // Je moc blízko, ale vidí ho -> asi by měl stát a střílet, 
                // nebo přidej logiku pro ústup. Zatím necháme stát.
                shouldMove = false;
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttackSequence();
            }
        }
        // V ostatních případech (nevidí ho, nebo ho právě uviděl a ještě si nadbíhá) -> shouldMove zůstane true

        // --- 4. APLIKACE POHYBU ---
        if (shouldMove)
        {
            agent.isStopped = false;
            // Stále aktualizujeme cíl na hráče, aby šel správným směrem i při nadbíhání
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    // ... ZBYTEK KÓDU (CheckLineOfSight, RotateGun, atd.) ZŮSTÁVÁ STEJNÝ JAKO V PŘEDCHOZÍ VERZI ...
    // Pro jistotu ho sem dávám znovu, abys to mohl celé zkopírovat.

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

    public void Shoot()
    {
        if (aimLine != null) aimLine.enabled = false;
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