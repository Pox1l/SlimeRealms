using UnityEngine;
using UnityEngine.AI;
using FMODUnity; // 🔥 1. Přidána knihovna pro FMOD

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject attackPrefab;

    // 🔥 NOVÉ: Odkaz na LineRenderer pro varování
    public LineRenderer warningLine;

    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public bool enableSpriteFlip = false;

    [Header("Audio")] // 🔥 2. Nová sekce pro Zvuk
    [Tooltip("Zvuk útoku. Pokud necháš prázdné, zahraje se Default Melee z Manageru.")]
    public EventReference attackSound;

    [Header("Range Offset")]
    public Vector2 centerOffset = Vector2.zero;

    [Header("Warning Settings")]
    public float warningRadius = 0.5f; // Jak velký má být ten červený kruh
    public int circleSegments = 20;    // Kolik hran má kruh (20 = hladký kruh)

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // 🔥 Zkusíme najít LineRenderer na firePointu, pokud není přiřazen
        if (warningLine == null && firePoint != null)
            warningLine = firePoint.GetComponent<LineRenderer>();

        if (warningLine != null) warningLine.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        RotatePivotToPlayer();

        // 🔥 NOVÉ: Pokud se chystá útok (běží animace), kreslíme kruh
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

    // 🔥 Funkce, která vykreslí kruh pomocí LineRendereru
    void DrawWarningCircle()
    {
        if (warningLine == null || firePoint == null) return;

        warningLine.positionCount = circleSegments;
        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float currentAngle = i * angleStep * Mathf.Deg2Rad;

            // Spočítáme pozici bodu na kružnici
            float x = Mathf.Cos(currentAngle) * warningRadius;
            float y = Mathf.Sin(currentAngle) * warningRadius;

            // Pozice bodu = Pozice firePointu + vypočítaný bod
            Vector3 pointPosition = firePoint.position + new Vector3(x, y, 0);

            warningLine.SetPosition(i, pointPosition);
        }
    }

    void StartAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack");

        // 🔥 Zapneme Line Renderer
        if (warningLine != null)
        {
            warningLine.enabled = true;
            DrawWarningCircle(); // Vykreslíme ho hned napoprvé
        }

        Invoke("FinishAttack", 1.0f);
    }

    public void FinishAttack()
    {
        isAttacking = false;
        // Pojistka: Vypneme čáru
        if (warningLine != null) warningLine.enabled = false;
        CancelInvoke("FinishAttack");
    }

    // 🔥 TADY SE DĚJE ÚTOK
    public void SpawnAttackHitbox()
    {
        // 🔥 Vypneme varování, protože teď přichází damage
        if (warningLine != null) warningLine.enabled = false;

        // --- AUDIO START: PŘEHRÁT ZVUK MELEE ÚTOKU ---
        if (AudioManager.instance != null)
        {
            // Zavoláme Manager, ať zahraje buď tento custom zvuk, nebo ten defaultní "švih"
            // Najdi řádek, kde voláš PlayMeleeAttack a přidej transform.position:
            AudioManager.instance.PlayMeleeAttack(attackSound, transform.position);
        }
        // --- AUDIO END ---

        if (attackPrefab == null || firePoint == null || fixedPoint == null) return;

        RotatePivotToPlayer();

        Quaternion correction = Quaternion.Euler(0, 0, -90);
        Instantiate(attackPrefab, firePoint.position, fixedPoint.rotation * correction);
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

        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, warningRadius); // Ukazuje velikost kruhu
        }
    }
}