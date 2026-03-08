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
    public GameObject attackPrefab;
    public GameObject projectilePrefab;
    public LineRenderer warningLine;
    public Transform fixedPoint;
    public Transform firePoint;
    public Animator animator;

    [Header("Phase 1 & 3 (Jump)")]
    public float jumpWarningRadius = 2.0f;
    public GameObject jumpEffectPrefab;

    [Header("Audio")]
    public EventReference movementSound;
    public EventReference aggroSound;
    private EventInstance moveInstance;
    private bool hasAggroed = false;

    [Header("Stats - Base")]
    public float moveSpeed = 3.5f;
    public float attackRange = 2.0f;
    public float rangedRange = 8.0f;
    public float attackCooldown = 1.5f;
    public float aggroRange = 15f;

    [Header("Stats - Phase 3 (Enraged)")]
    public float fastMoveSpeed = 5.5f;
    public float fastAttackCooldown = 0.8f;
    public float fastAnimationSpeed = 1.5f;

    [Header("Visuals")]
    public float warningRadius = 0.5f;
    public int circleSegments = 20;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;
    private bool phase3BuffApplied = false;

    public static event Action OnMeleeAttack;
    public static event Action OnRangedAttack;
    public static event Action OnBossLand;

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
    }

    void OnEnable()
    {
        currentStage = BossStage.Phase1;
        phase3BuffApplied = false;
        isAttacking = false;
        hasAggroed = false;
        lastAttackTime = -999f;

        if (agent != null) agent.speed = moveSpeed;
        if (animator != null) animator.speed = 1.0f;

        if (warningLine != null) warningLine.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

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
            if (currentStage == BossStage.Phase2) DrawWarningLine(rangedRange);
            else if (currentStage == BossStage.Phase1 || currentStage == BossStage.Phase3) DrawWarningCircle(jumpWarningRadius);
            else DrawWarningCircle(warningRadius);
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        float stopDistance = (currentStage == BossStage.Phase2) ? rangedRange : attackRange;
        if (currentStage == BossStage.Phase2 && distance <= attackRange) stopDistance = attackRange;

        if (!isAttacking)
        {
            if (!hasAggroed && distance < aggroRange) { PlayAggroSound(); hasAggroed = true; }

            if (distance <= stopDistance)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                UpdateMoveSound(false);

                Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
                if (animator != null)
                {
                    animator.SetFloat("Horizontal", dirToPlayer.x);
                    animator.SetFloat("Vertical", dirToPlayer.y);
                    animator.SetFloat("Speed", 0);
                }

                float currentCooldown = (currentStage == BossStage.Phase3) ? fastAttackCooldown : attackCooldown;
                if (Time.time >= lastAttackTime + currentCooldown) DecideAttack(distance);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                UpdateMoveSound(true);
                SetAnimator(agent.velocity);
            }
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

        if (hpPercent <= 0.33f)
        {
            currentStage = BossStage.Phase3;
            if (!phase3BuffApplied)
            {
                agent.speed = fastMoveSpeed;
                if (animator != null)
                {
                    animator.speed = fastAnimationSpeed;
                    animator.SetTrigger("Enrage");
                }
                phase3BuffApplied = true;
            }
        }
        else if (hpPercent <= 0.66f) currentStage = BossStage.Phase2;
        else currentStage = BossStage.Phase1;
    }

    void DecideAttack(float distance)
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        if (animator != null) animator.SetFloat("Speed", 0);

        switch (currentStage)
        {
            case BossStage.Phase1:
                StartJumpAttack();
                break;
            case BossStage.Phase2:
                if (distance <= attackRange) StartJumpAttack();
                else StartRangedAttack();
                break;
            case BossStage.Phase3:
                StartJumpAttack();
                break;
        }
    }

    void StartJumpAttack()
    {
        if (animator != null) animator.SetTrigger("Jump");
        if (warningLine != null) warningLine.enabled = true;
    }

    void StartRangedAttack()
    {
        if (animator != null) animator.SetTrigger("AttackRanged");
        if (warningLine != null) warningLine.enabled = true;
    }

    public void AnimEvent_LandHit()
    {
        if (warningLine != null) warningLine.enabled = false;

        if (fixedPoint != null)
        {
            if (jumpEffectPrefab != null) Instantiate(jumpEffectPrefab, fixedPoint.position, Quaternion.identity);

            if (attackPrefab != null)
            {
                RotatePivotToPlayer();
                Instantiate(attackPrefab, fixedPoint.position, fixedPoint.rotation);
            }
        }

        OnBossLand?.Invoke();
        OnMeleeAttack?.Invoke();
        FinishAttack();
    }

    public void AnimEvent_MeleeHit()
    {
        if (warningLine != null) warningLine.enabled = false;
        if (attackPrefab != null && firePoint != null)
        {
            RotatePivotToPlayer();
            Instantiate(attackPrefab, firePoint.position, fixedPoint.rotation);
        }
        OnMeleeAttack?.Invoke();
        FinishAttack();
    }

    public void AnimEvent_RangedHit()
    {
        if (warningLine != null) warningLine.enabled = false;
        if (projectilePrefab != null && firePoint != null)
        {
            RotatePivotToPlayer();
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, fixedPoint.rotation);
            if (proj.GetComponent<Rigidbody2D>())
                proj.GetComponent<Rigidbody2D>().velocity = fixedPoint.right * 10f;
        }
        OnRangedAttack?.Invoke();
        FinishAttack();
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (warningLine != null) warningLine.enabled = false;
    }

    void UpdateMoveSound(bool isMoving) { if (moveInstance.isValid()) { moveInstance.getPlaybackState(out PLAYBACK_STATE state); if (isMoving && state != PLAYBACK_STATE.PLAYING) moveInstance.start(); else if (!isMoving && state == PLAYBACK_STATE.PLAYING) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); } }
    void StopMoveSound() { if (moveInstance.isValid()) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); }
    void PlayAggroSound() { if (!aggroSound.IsNull) RuntimeManager.PlayOneShot(aggroSound, transform.position); }

    void RotatePivotToPlayer()
    {
        if (fixedPoint == null || playerTransform == null) return;
        Vector2 dir = playerTransform.position - fixedPoint.position;

        if (dir.sqrMagnitude < 0.1f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fixedPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void SetAnimator(Vector2 velocity)
    {
        if (animator == null) return;
        animator.SetFloat("Horizontal", velocity.x);
        animator.SetFloat("Vertical", velocity.y);
        animator.SetFloat("Speed", velocity.magnitude);
    }

    void DrawWarningCircle(float radius)
    {
        if (warningLine == null) return;
        warningLine.positionCount = circleSegments;
        float angleStep = 360f / circleSegments;
        for (int i = 0; i < circleSegments; i++)
        {
            float currentAngle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            Vector3 centerPos = ((currentStage == BossStage.Phase1 || currentStage == BossStage.Phase3) && fixedPoint != null) ? fixedPoint.position : firePoint.position;
            warningLine.SetPosition(i, centerPos + new Vector3(x, y, 0));
        }
    }

    void DrawWarningLine(float length) { if (warningLine == null || firePoint == null) return; warningLine.positionCount = 2; warningLine.SetPosition(0, firePoint.position); warningLine.SetPosition(1, firePoint.position + (fixedPoint.right * length)); }

    void OnDisable() { StopMoveSound(); }
    void OnDestroy() { StopMoveSound(); moveInstance.release(); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, jumpWarningRadius);
    }
}