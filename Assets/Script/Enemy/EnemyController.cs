using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float chaseRange = 15f;

    [Header("Audio (FMOD)")]
    public EventReference movementSound;
    public EventReference aggroSound;

    // 🔥 Cooldown pro zvuk
    [SerializeField] private float aggroSoundCooldown = 3f;
    private float lastAggroSoundTime = -999f;

    private float currentDetectionRange;
    private Vector3 homePosition;

    private EventInstance moveInstance;
    private bool isChasing = false;
    private bool isInCombatRegistry = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;

        currentDetectionRange = aggroRange;

        FindPlayer();
    }

    void Start()
    {
        if (!movementSound.IsNull)
        {
            moveInstance = RuntimeManager.CreateInstance(movementSound);
            RuntimeManager.AttachInstanceToGameObject(moveInstance, gameObject, GetComponent<Rigidbody2D>());
        }
    }

    void OnEnable()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (homePosition == Vector3.zero) homePosition = transform.position;
        currentDetectionRange = aggroRange;
        isChasing = false;
        isInCombatRegistry = false;
    }

    void OnDisable()
    {
        StopMoveSound();
        RemoveFromCombatRegistry();
    }

    void OnDestroy()
    {
        StopMoveSound();
        moveInstance.release();
        RemoveFromCombatRegistry();
    }

    public void OnHitAggro()
    {
        currentDetectionRange = chaseRange;

        if (!isChasing)
        {
            PlayAggroSound();
            isChasing = true;
            AddToCombatRegistry();
        }
    }

    public void SetHomePosition(Vector3 position)
    {
        homePosition = position;
    }

    void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            StopMoveSound();
            return;
        }

        if (player == null)
        {
            FindPlayer();
            StopMoveSound();
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToHome = Vector2.Distance(transform.position, homePosition);

        Vector3 targetPosition;

        // --- Logika pohybu a Aggra ---
        if (distanceToPlayer <= currentDetectionRange)
        {
            // PRÁVĚ SI HO VŠIML
            if (!isChasing)
            {
                PlayAggroSound();
                isChasing = true;
                AddToCombatRegistry();
            }

            targetPosition = player.position;
        }
        else
        {
            // HRÁČ UTEKL - VRACÍME SE
            if (isChasing)
            {
                isChasing = false;
                RemoveFromCombatRegistry();
            }

            // Zpět na základní dosah
            currentDetectionRange = aggroRange;

            if (distanceToHome <= stopDistance)
            {
                agent.ResetPath();
                SetAnimator(Vector2.zero);
                StopMoveSound();
                return;
            }

            targetPosition = homePosition;
        }

        if (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            agent.SetDestination(targetPosition);
            UpdateMoveSound(true);
        }
        else
        {
            agent.ResetPath();
            UpdateMoveSound(false);
        }

        Vector2 velocity2D = new Vector2(agent.velocity.x, agent.velocity.y);
        SetAnimator(velocity2D);
    }

    void AddToCombatRegistry()
    {
        if (!isInCombatRegistry && AudioManager.instance != null)
        {
            AudioManager.instance.AddAggro();
            isInCombatRegistry = true;
        }
    }

    void RemoveFromCombatRegistry()
    {
        if (isInCombatRegistry && AudioManager.instance != null)
        {
            AudioManager.instance.RemoveAggro();
            isInCombatRegistry = false;
        }
    }

    void UpdateMoveSound(bool isMoving)
    {
        if (moveInstance.isValid())
        {
            moveInstance.getPlaybackState(out PLAYBACK_STATE state);

            if (isMoving && state != PLAYBACK_STATE.PLAYING)
            {
                moveInstance.start();
            }
            else if (!isMoving && state == PLAYBACK_STATE.PLAYING)
            {
                moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    void StopMoveSound()
    {
        if (moveInstance.isValid())
        {
            moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void PlayAggroSound()
    {
        // 🔥 UPRAVENO: Už nekontrolujeme, jestli je aggroSound.IsNull tady, 
        // to rozhodne až Manager (protože teď chceme, aby mohl hrát default).
        if (Time.time >= lastAggroSoundTime + aggroSoundCooldown)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayAggroSound(aggroSound, transform.position);
            }
            lastAggroSoundTime = Time.time;
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void SetAnimator(Vector2 dir)
    {
        if (animator == null) return;
        animator.SetFloat("Horizontal", dir.x);
        animator.SetFloat("Vertical", dir.y);
        animator.SetFloat("Speed", dir.magnitude);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}