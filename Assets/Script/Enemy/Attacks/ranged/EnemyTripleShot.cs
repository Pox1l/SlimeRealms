using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity; // 🔥 1. Nutné pro FMOD

public class EnemyTripleShot : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    // 🔥 TOTO je nyní střed pro výpočet vzdálenosti
    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;
    public LineRenderer aimLine;

    [Header("Combat Ranges (from Fixed Point)")]
    public float attackRange = 7f;
    public float stoppingDistance = 4f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f;
    public float aimLineLength = 10f;

    [Header("Audio")] // 🔥 2. Nová sekce pro Zvuk
    [Tooltip("Zvuk výstřelu. Hraje při každém náboji v dávce.")]
    public EventReference shootSound;

    [Header("Timing (No Animation)")]
    public float shootDelay = 0.5f;

    [Header("Burst Settings")]
    public int shotCount = 3;
    public float timeBetweenShots = 0.3f;

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
        // Pojistka: pokud nemáme fixedPoint, nemůžeme počítat vzdálenost od něj
        if (playerTransform == null || fixedPoint == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        if (isAttacking)
        {
            RotateGunToPlayer();
            if (aimLine != null && aimLine.enabled)
            {
                UpdateAimLinePosition();
            }
        }

        // 🔥 ZMĚNA: Vzdálenost se počítá od fixedPoint.position namísto transform.position
        float distanceToPlayer = Vector2.Distance(fixedPoint.position, playerTransform.position);

        if (!isAttacking)
        {
            if (distanceToPlayer <= stoppingDistance)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }
        }

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttackSequence();
        }
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

        // 🔥 PŘIDÁNO: Okamžité zastavení agenta při začátku útoku
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null) animator.SetTrigger("Attack");

        // Spustíme Coroutine rovnou, časování si řeší ona
        Shoot();
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    public void Shoot()
    {
        StartCoroutine(BurstFireRoutine());
    }

    IEnumerator BurstFireRoutine()
    {
        for (int i = 0; i < shotCount; i++)
        {
            if (this == null || !gameObject.activeInHierarchy) yield break;

            // 1. Ukázat čáru a začít mířit
            if (aimLine != null)
            {
                aimLine.enabled = true;
                RotateGunToPlayer();
                UpdateAimLinePosition();
            }

            // 2. Počkat před výstřelem (první rána čeká shootDelay, další timeBetweenShots)
            float waitTime = (i == 0) ? shootDelay : timeBetweenShots;
            yield return new WaitForSeconds(waitTime);

            // 3. Skrýt čáru těsně při výstřelu
            if (aimLine != null) aimLine.enabled = false;

            // 4. Samotný výstřel
            if (projectilePrefab != null && firePoint != null && fixedPoint != null)
            {
                // --- AUDIO START ---
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlayRangedAttack(shootSound, firePoint.position);
                }
                // --- AUDIO END ---

                RotateGunToPlayer();
                Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
            }
        }

        FinishAttack();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    // 🔥 ZMĚNA: Gizmos se nyní kreslí kolem fixedPointu
    void OnDrawGizmosSelected()
    {
        Vector3 rangeOrigin = fixedPoint != null ? fixedPoint.position : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rangeOrigin, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rangeOrigin, stoppingDistance);
    }
}