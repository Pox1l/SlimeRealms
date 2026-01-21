using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    // ... (Všechny proměnné zůstávají stejné) ...
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
    // public float jumpDuration = 1.0f; // ❌ UŽ NENÍ POTŘEBA (řídí to animace)
    public GameObject jumpEffectPrefab;

    [Header("Warning Settings")]
    public float warningRadius = 0.5f;
    public int circleSegments = 20;

    private Transform playerTransform;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    // ... (Awake, Start, Update jsou stejné) ...

    void Awake() { /* Stejné jako předtím */ agent = GetComponent<NavMeshAgent>(); if (animator == null) animator = GetComponent<Animator>(); if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>(); if (bossHealth == null) bossHealth = GetComponent<BossHealth>(); agent.updateRotation = false; agent.updateUpAxis = false; agent.speed = moveSpeed; }
    void Start() { /* Stejné jako předtím */ if (!movementSound.IsNull) { moveInstance = RuntimeManager.CreateInstance(movementSound); RuntimeManager.AttachInstanceToGameObject(moveInstance, gameObject, GetComponent<Rigidbody2D>()); } if (warningLine == null && firePoint != null) warningLine = firePoint.GetComponent<LineRenderer>(); if (warningLine != null) warningLine.enabled = false; GameObject player = GameObject.FindGameObjectWithTag("Player"); if (player != null) playerTransform = player.transform; }

    void Update()
    {
        // ... (Celý Update zůstává stejný) ...
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

    // ... (Audio metody a CheckBossPhase jsou stejné) ...
    void UpdateMoveSound(bool isMoving) { if (moveInstance.isValid()) { moveInstance.getPlaybackState(out PLAYBACK_STATE state); if (isMoving && state != PLAYBACK_STATE.PLAYING) moveInstance.start(); else if (!isMoving && state == PLAYBACK_STATE.PLAYING) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); } }
    void StopMoveSound() { if (moveInstance.isValid()) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); }
    void PlayAggroSound() { if (!aggroSound.IsNull) RuntimeManager.PlayOneShot(aggroSound, transform.position); }
    void OnDisable() => StopMoveSound();
    void OnDestroy() { StopMoveSound(); moveInstance.release(); }
    void CheckBossPhase() { if (bossHealth != null && bossHealth.maxHealth > 0) { float hpPercent = (float)bossHealth.currentHealth / bossHealth.maxHealth; if (hpPercent <= 0.5f) currentStage = BossStage.Phase2; } }

    void DecideAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        if (currentStage == BossStage.Phase1) StartMeleeAttack();
        else if (currentStage == BossStage.Phase2) StartJumpAttack();
    }

    // --- FÁZE 1: MELEE ---
    void StartMeleeAttack()
    {
        if (animator != null) animator.SetTrigger("Attack");
        if (warningLine != null) warningLine.enabled = true;

        // U Melee útoku zatím necháme Invoke, pokud tam taky nemáš Event
        Invoke("SpawnAttackHitbox", 0.8f);
        Invoke("FinishAttack", 1.0f);
    }

    public void SpawnAttackHitbox()
    {
        if (warningLine != null) warningLine.enabled = false;
        if (attackPrefab == null || firePoint == null || fixedPoint == null) return;
        RotatePivotToPlayer();
        Quaternion correction = Quaternion.Euler(0, 0, 0);
        Instantiate(attackPrefab, firePoint.position, fixedPoint.rotation * correction);
    }

    // --- FÁZE 2: JUMP (S ANIMATION EVENTEM) ---
    void StartJumpAttack()
    {
        if (animator != null) animator.SetTrigger("Jump");

        // Zapneme warning kruh
        if (warningLine != null) warningLine.enabled = true;

        // ❌ UŽ ŽÁDNÝ INVOKE! Čekáme na Event z animace.
    }

    // 🔥 TUTO FUNKCI VYBER V ANIMATION EVENTU (v okně Animation)
    public void AnimEvent_LandHit()
    {
        // 1. Vypnout warning (už dopadl)
        if (warningLine != null) warningLine.enabled = false;

        // 2. Efekt
        if (jumpEffectPrefab != null) Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);

        // 3. Damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, jumpDamageRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Místo Debug.Log to napoj na HP hráče:
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(jumpDamageAmount);
                }
            }
        }

        // 4. Ukončit stav útoku
        FinishAttack();
    }

    public void FinishAttack()
    {
        isAttacking = false;
        if (warningLine != null) warningLine.enabled = false;
    }

    // --- POMOCNÉ FUNKCE (Zůstávají) ---
    void DrawWarningCircle(float radius) { if (warningLine == null || firePoint == null) return; warningLine.positionCount = circleSegments; float angleStep = 360f / circleSegments; for (int i = 0; i < circleSegments; i++) { float currentAngle = i * angleStep * Mathf.Deg2Rad; float x = Mathf.Cos(currentAngle) * radius; float y = Mathf.Sin(currentAngle) * radius; Vector3 centerPos = (currentStage == BossStage.Phase2) ? transform.position : firePoint.position; warningLine.SetPosition(i, centerPos + new Vector3(x, y, 0)); } }
    void RotatePivotToPlayer() { if (fixedPoint == null || playerTransform == null) return; Vector2 dir = playerTransform.position - fixedPoint.position; float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; fixedPoint.rotation = Quaternion.Euler(0, 0, angle); }
    void SetAnimator(Vector2 velocity) { if (animator == null) return; animator.SetFloat("Horizontal", velocity.x); animator.SetFloat("Vertical", velocity.y); animator.SetFloat("Speed", velocity.magnitude); }
    void OnDrawGizmos() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, jumpDamageRadius); }
}