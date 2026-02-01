using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class Boss3Controller : MonoBehaviour
{
    public enum BossStage { Phase1, Phase2, Phase3 }

    [Header("Boss Status")]
    public BossStage currentStage = BossStage.Phase1;
    public BossHealth bossHealth;

    [Header("References")]
    public GameObject attackPrefab;     // Melee hitbox (Stomp)
    public GameObject projectilePrefab; // Ranged (Web/Poison)
    public LineRenderer warningLine;
    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;

    [Header("Audio")]
    public EventReference movementSound;
    public EventReference aggroSound;
    private EventInstance moveInstance;
    private bool hasAggroed = false;

    [Header("Stats - Base")]
    public float moveSpeed = 3.5f;
    public float attackRange = 2.0f;    // Blízko (Stomp)
    public float rangedRange = 8.0f;    // Dálka (Fáze 2)
    public float attackCooldown = 1.5f;
    public float aggroRange = 15f;      // Kdy si všimne hráèe

    [Header("Stats - Phase 3 (Enraged)")]
    public float fastMoveSpeed = 5.5f;
    public float fastAttackCooldown = 0.8f;

    [Header("Visuals")]
    public float warningRadius = 0.5f;
    public int circleSegments = 20;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;
    private bool phase3BuffApplied = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (bossHealth == null) bossHealth = GetComponent<BossHealth>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
    }

    void Start()
    {
        if (!movementSound.IsNull) { moveInstance = RuntimeManager.CreateInstance(movementSound); RuntimeManager.AttachInstanceToGameObject(moveInstance, gameObject, GetComponent<Rigidbody2D>()); }
        if (warningLine == null && firePoint != null) warningLine = firePoint.GetComponent<LineRenderer>();
        if (warningLine != null) warningLine.enabled = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) { StopMoveSound(); return; }

        CheckBossPhase();
        RotatePivotToPlayer();

        // Kreslení warning kruhu (pokud útoèíme)
        if (isAttacking && warningLine != null && warningLine.enabled)
            DrawWarningCircle(warningRadius);

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // --- Logika pohybu a zastavení ---
        // Fáze 2 zastavuje dál (aby støílel), Fáze 1 a 3 musí jít až k tìlu
        float stopDistance = (currentStage == BossStage.Phase2) ? rangedRange : attackRange;

        // Výjimka pro Fázi 2: Pokud je hráè moc blízko, boss se zastaví, aby dal melee
        if (currentStage == BossStage.Phase2 && distance <= attackRange) stopDistance = attackRange;

        if (!isAttacking)
        {
            if (!hasAggroed && distance < aggroRange) { PlayAggroSound(); hasAggroed = true; }

            if (distance <= stopDistance)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                UpdateMoveSound(false);

                // Kontrola cooldownu (ve fázi 3 je rychlejší)
                float currentCooldown = (currentStage == BossStage.Phase3) ? fastAttackCooldown : attackCooldown;

                if (Time.time >= lastAttackTime + currentCooldown)
                    DecideAttack(distance);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                UpdateMoveSound(true);
            }
            SetAnimator(agent.velocity);
        }
        else
        {
            agent.isStopped = true;
            UpdateMoveSound(false);
        }
    }

    void CheckBossPhase()
    {
        if (bossHealth == null) return;

        float hpPercent = (float)bossHealth.currentHealth / bossHealth.maxHealth;

        // Fáze 3: Pod 33% HP
        if (hpPercent <= 0.33f)
        {
            currentStage = BossStage.Phase3;
            if (!phase3BuffApplied)
            {
                agent.speed = fastMoveSpeed;
                phase3BuffApplied = true;
                animator.SetTrigger("Enrage");
            }
        }
        // Fáze 2: Pod 66% HP
        else if (hpPercent <= 0.66f)
        {
            currentStage = BossStage.Phase2;
        }
        // Fáze 1: Nad 66% HP
        else
        {
            currentStage = BossStage.Phase1;
        }
    }

    void DecideAttack(float distance)
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        switch (currentStage)
        {
            case BossStage.Phase1:
                StartMeleeAttack(); // Jen melee
                break;

            case BossStage.Phase2:
                if (distance <= attackRange)
                    StartMeleeAttack(); // Hráè je blízko -> Braò se!
                else
                    StartRangedAttack(); // Hráè je daleko -> Støílej!
                break;

            case BossStage.Phase3:
                StartMeleeAttack(); // Rychlé melee (cooldown øeší Update)
                break;
        }
    }

    // --- ÚTOKY ---

    void StartMeleeAttack()
    {
        if (animator != null) animator.SetTrigger("Attack");
        if (warningLine != null) warningLine.enabled = true;

        float delay = (currentStage == BossStage.Phase3) ? 0.4f : 0.8f;
        Invoke("SpawnMeleeHitbox", delay);
        Invoke("FinishAttack", delay + 0.2f);
    }

    void StartRangedAttack()
    {
        if (animator != null) animator.SetTrigger("AttackRanged");
        if (warningLine != null) warningLine.enabled = true;

        Invoke("SpawnProjectile", 0.5f);
        Invoke("FinishAttack", 1.0f);
    }

    public void SpawnMeleeHitbox()
    {
        if (warningLine != null) warningLine.enabled = false;
        if (attackPrefab != null && firePoint != null)
        {
            RotatePivotToPlayer();
            Instantiate(attackPrefab, firePoint.position, fixedPoint.rotation);
        }
    }

    public void SpawnProjectile()
    {
        if (warningLine != null) warningLine.enabled = false;
        if (projectilePrefab != null && firePoint != null)
        {
            RotatePivotToPlayer();
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
            if (proj.GetComponent<Rigidbody2D>())
                proj.GetComponent<Rigidbody2D>().velocity = fixedPoint.right * 10f;
        }
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (warningLine != null) warningLine.enabled = false;
    }

    // --- AUDIO & UTILS ---
    void UpdateMoveSound(bool isMoving) { if (moveInstance.isValid()) { moveInstance.getPlaybackState(out PLAYBACK_STATE state); if (isMoving && state != PLAYBACK_STATE.PLAYING) moveInstance.start(); else if (!isMoving && state == PLAYBACK_STATE.PLAYING) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); } }
    void StopMoveSound() { if (moveInstance.isValid()) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); }
    void PlayAggroSound() { if (!aggroSound.IsNull) RuntimeManager.PlayOneShot(aggroSound, transform.position); }
    void RotatePivotToPlayer() { if (fixedPoint == null || playerTransform == null) return; Vector2 dir = playerTransform.position - fixedPoint.position; float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; fixedPoint.rotation = Quaternion.Euler(0, 0, angle); }
    void SetAnimator(Vector2 velocity) { if (animator == null) return; animator.SetFloat("Speed", velocity.magnitude); }
    void DrawWarningCircle(float radius) { if (warningLine == null || firePoint == null) return; warningLine.positionCount = circleSegments; float angleStep = 360f / circleSegments; for (int i = 0; i < circleSegments; i++) { float currentAngle = i * angleStep * Mathf.Deg2Rad; float x = Mathf.Cos(currentAngle) * radius; float y = Mathf.Sin(currentAngle) * radius; warningLine.SetPosition(i, firePoint.position + new Vector3(x, y, 0)); } }
    void OnDisable() => StopMoveSound();
    void OnDestroy() { StopMoveSound(); moveInstance.release(); }

    // --- GIZMOS (Visualizace v Editoru) ---
    void OnDrawGizmosSelected()
    {
        // 1. Melee Range (Èervená)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Ranged Range (Tyrkysová)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        // 3. Aggro Range (Žlutá)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}