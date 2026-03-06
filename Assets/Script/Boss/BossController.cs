using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;
using System;

// Téma: BossController - Úprava Slash útoku na Animation Event
[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    public enum BossStage { Phase1, Phase2 }
    [Header("Boss Status")]
    public BossStage currentStage = BossStage.Phase1;
    public BossHealth bossHealth;

    [Header("References")]
    public GameObject attackPrefab;
    public LineRenderer warningLine;
    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Audio (FMOD)")]
    public EventReference movementSound;
    public EventReference aggroSound;
    private EventInstance moveInstance;
    private bool hasAggroed = false;

    [Header("Settings")]
    public float moveSpeed = 3.5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Phase 2: Jump Attack")]
    public float jumpDamageRadius = 3f;
    public int jumpDamageAmount = 10;
    public GameObject jumpEffectPrefab;

    [Header("Warning Settings")]
    public float warningRadius = 0.5f;
    public int circleSegments = 20;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    public static event Action OnBossLand;
    public static event Action OnPhase2Start;
    private bool phase2SignalSent = false;


    void Awake() { agent = GetComponent<NavMeshAgent>(); if (animator == null) animator = GetComponent<Animator>(); if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>(); if (bossHealth == null) bossHealth = GetComponent<BossHealth>(); agent.updateRotation = false; agent.updateUpAxis = false; agent.speed = moveSpeed; }

    void Start()
    {
        if (!movementSound.IsNull)
        {
            moveInstance = RuntimeManager.CreateInstance(movementSound);
            RuntimeManager.AttachInstanceToGameObject(moveInstance, gameObject, GetComponent<Rigidbody2D>());
        }
        if (warningLine == null && firePoint != null) warningLine = firePoint.GetComponent<LineRenderer>();
        if (warningLine != null) warningLine.enabled = false;
    }

    // 🔥 PŘIDÁNO: Reset všech hodnot při spawnu z poolu
    void OnEnable()
    {
        currentStage = BossStage.Phase1;
        phase2SignalSent = false;
        isAttacking = false;
        hasAggroed = false;
        lastAttackTime = -999f;

        if (warningLine != null) warningLine.enabled = false;

        // Znovu najde hráče
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Reset agenta, pokud už je na NavMeshi
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) { StopMoveSound(); return; }

        CheckBossPhase();
        RotatePivotToPlayer();

        if (isAttacking && warningLine != null && warningLine.enabled)
        {
            float currentRadius = (currentStage == BossStage.Phase2) ? jumpDamageRadius : warningRadius;
            DrawWarningCircle(currentRadius);
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (!isAttacking)
        {
            if (!hasAggroed && distance < 15f) { PlayAggroSound(); hasAggroed = true; }

            if (distance <= attackRange)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                UpdateMoveSound(false);
                if (Time.time >= lastAttackTime + attackCooldown) DecideAttack();
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                UpdateMoveSound(true);
            }
            SetAnimator(agent.velocity); // Animace pohybu
        }
        else
        {
            agent.isStopped = true;
            UpdateMoveSound(false);
        }
    }

    void UpdateMoveSound(bool isMoving) { if (moveInstance.isValid()) { moveInstance.getPlaybackState(out PLAYBACK_STATE state); if (isMoving && state != PLAYBACK_STATE.PLAYING) moveInstance.start(); else if (!isMoving && state == PLAYBACK_STATE.PLAYING) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); } }
    void StopMoveSound() { if (moveInstance.isValid()) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); }
    void PlayAggroSound() { if (!aggroSound.IsNull) RuntimeManager.PlayOneShot(aggroSound, transform.position); }

    // ZMĚNĚNO: Původní OnDisable bylo zapsáno lambdou, raději jsem ho rozepsal kvůli přehlednosti
    void OnDisable()
    {
        StopMoveSound();
    }

    void OnDestroy() { StopMoveSound(); moveInstance.release(); }

    void CheckBossPhase()
    {
        if (bossHealth != null && bossHealth.maxHealth > 0)
        {
            float hpPercent = (float)bossHealth.currentHealth / bossHealth.maxHealth;

            if (hpPercent <= 0.5f)
            {
                currentStage = BossStage.Phase2;

                if (!phase2SignalSent)
                {
                    OnPhase2Start?.Invoke();
                    phase2SignalSent = true;
                }
            }
        }
    }

    void DecideAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        if (currentStage == BossStage.Phase1) StartMeleeAttack();
        else if (currentStage == BossStage.Phase2) StartJumpAttack();
    }

    void StartMeleeAttack()
    {
        if (animator != null) animator.SetTrigger("Attack");
        if (warningLine != null) warningLine.enabled = true;
    }

    public void SpawnAttackHitbox()
    {
        if (warningLine != null) warningLine.enabled = false;

        if (attackPrefab == null || firePoint == null || fixedPoint == null) return;
        RotatePivotToPlayer();
        Quaternion correction = Quaternion.Euler(0, 0, 0);
        Instantiate(attackPrefab, firePoint.position, fixedPoint.rotation * correction);
    }

    void StartJumpAttack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0);
            animator.SetTrigger("Jump");
        }

        if (warningLine != null) warningLine.enabled = true;
    }

    public void AnimEvent_LandHit()
    {
        if (warningLine != null) warningLine.enabled = false;

        if (jumpEffectPrefab != null) Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);

        OnBossLand?.Invoke();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, jumpDamageRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                if (playerStats != null) playerStats.TakeDamage(jumpDamageAmount, transform);
            }
        }

        FinishAttack();
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (warningLine != null) warningLine.enabled = false;
    }

    void DrawWarningCircle(float radius) { if (warningLine == null || firePoint == null) return; warningLine.positionCount = circleSegments; float angleStep = 360f / circleSegments; for (int i = 0; i < circleSegments; i++) { float currentAngle = i * angleStep * Mathf.Deg2Rad; float x = Mathf.Cos(currentAngle) * radius; float y = Mathf.Sin(currentAngle) * radius; Vector3 centerPos = (currentStage == BossStage.Phase2) ? transform.position : firePoint.position; warningLine.SetPosition(i, centerPos + new Vector3(x, y, 0)); } }
    void RotatePivotToPlayer() { if (fixedPoint == null || playerTransform == null) return; Vector2 dir = playerTransform.position - fixedPoint.position; float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; fixedPoint.rotation = Quaternion.Euler(0, 0, angle); }
    void SetAnimator(Vector2 velocity) { if (animator == null) return; animator.SetFloat("Horizontal", velocity.x); animator.SetFloat("Vertical", velocity.y); animator.SetFloat("Speed", velocity.magnitude); }
    void OnDrawGizmos() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, jumpDamageRadius); }
}