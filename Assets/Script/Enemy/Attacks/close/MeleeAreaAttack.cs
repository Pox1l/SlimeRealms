using UnityEngine;
using UnityEngine.AI;

public class MeleeAreaAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject attackPrefab;

    public LineRenderer warningLine;

    public Transform fixedPoint; // 🔥 ZDE se kreslí kruh i spawnuje útok
    public Transform firePoint;  // (Teď už se prakticky nepoužívá, můžeš tam nechat střed slima)
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public bool enableSpriteFlip = false;

    [Header("Timing")]
    public float attackDelay = 0.5f; // Zpoždění útoku, když nemáš animaci

    [Header("Range Offset")]
    public Vector2 centerOffset = Vector2.zero;

    [Header("Warning Settings")]
    public float warningRadius = 0.5f;
    public int circleSegments = 20;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Zkusíme najít LineRenderer na fixedPointu nebo firePointu
        if (warningLine == null)
        {
            if (fixedPoint != null) warningLine = fixedPoint.GetComponent<LineRenderer>();
            if (warningLine == null && firePoint != null) warningLine = firePoint.GetComponent<LineRenderer>();
        }

        if (warningLine != null) warningLine.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        RotatePivotToPlayer();

        if (isAttacking && warningLine != null && warningLine.enabled)
        {
            DrawWarningCircle();
        }

        Vector2 rangeCenter = (Vector2)transform.position + centerOffset;
        float distance = Vector2.Distance(rangeCenter, playerTransform.position);

        if (!isAttacking)
        {
            if (distance <= attackRange)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                if (enableSpriteFlip) FacePlayer();

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartAttackSequence();
                }
            }
            else
            {
                agent.isStopped = false;
                if (enableSpriteFlip) FacePlayer();
            }
        }
    }

    void RotatePivotToPlayer()
    {
        if (fixedPoint == null || playerTransform == null) return;
        Vector2 dir = playerTransform.position - fixedPoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fixedPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void DrawWarningCircle()
    {
        // 🔥 ZMĚNA: Kontrolujeme fixedPoint místo firePoint
        if (warningLine == null || fixedPoint == null) return;

        warningLine.positionCount = circleSegments;
        warningLine.loop = true;
        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float currentAngle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(currentAngle) * warningRadius;
            float y = Mathf.Sin(currentAngle) * warningRadius;

            // 🔥 ZMĚNA: Kruh se kreslí kolem fixedPointu
            Vector3 pointPosition = fixedPoint.position + new Vector3(x, y, 0);
            warningLine.SetPosition(i, pointPosition);
        }
    }

    void StartAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack"); // Tohle spouští animaci

        if (warningLine != null)
        {
            warningLine.enabled = true;
            DrawWarningCircle();
        }

        // ❌ SMAZAT TOTO (Spouští to teď Animation Event):
        // Invoke("SpawnAttackHitbox", attackDelay); 

        // Tohle tu nech, aby se útok ukončil a slime se mohl zase hýbat
        Invoke("FinishAttack", 1.0f); // Čas uprav podle délky animace
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (warningLine != null) warningLine.enabled = false;
        CancelInvoke("FinishAttack");
    }

    public void SpawnAttackHitbox()
    {
        if (warningLine != null) warningLine.enabled = false;

        if (attackPrefab == null || fixedPoint == null) return;

        // Spawn na pozici fixedPointu
        Instantiate(attackPrefab, fixedPoint.position, Quaternion.identity);
    }

    void FacePlayer()
    {
        if (spriteRenderer == null || playerTransform == null) return;
        if (playerTransform.position.x > transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 rangeCenter = transform.position + (Vector3)centerOffset;
        Gizmos.DrawWireSphere(rangeCenter, attackRange);

        // 🔥 ZMĚNA: Gizmos (modrý kruh) se teď ukazuje kolem fixedPointu
        if (fixedPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(fixedPoint.position, warningRadius);
        }
    }
}