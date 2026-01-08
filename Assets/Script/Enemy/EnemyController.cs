using UnityEngine;
using UnityEngine.AI;
using FMODUnity;  // 1. Nutné pro FMOD
using FMOD.Studio; // 2. Nutné pro práci s Instancemi

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
    public EventReference movementSound; // Sem dej smyčku chůze/lezení
    public EventReference aggroSound;    // Sem dej zvuk "zavřeštění", když tě uvidí

    private float currentDetectionRange;
    private Vector3 homePosition;

    // Proměnné pro audio
    private EventInstance moveInstance;
    private bool isChasing = false; // Aby aggro zvuk nehrál pořád dokola

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
        // 3. Vytvoření instance pro pohyb (aby šla stopnout)
        if (!movementSound.IsNull)
        {
            moveInstance = RuntimeManager.CreateInstance(movementSound);
            // Připneme zvuk k nepříteli, aby byl 3D
            // OPRAVA: Místo 'transform' posíláme 'gameObject'
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
        isChasing = false; // Reset stavu
    }

    void OnDisable()
    {
        // Pojistka: Když se nepřítel vypne, vypneme zvuk
        StopMoveSound();
    }

    void OnDestroy()
    {
        // Úklid paměti
        StopMoveSound();
        moveInstance.release();
    }

    public void OnHitAggro()
    {
        currentDetectionRange = chaseRange;

        // Pokud nás trefil a my jsme o něm nevěděli, přehrajeme aggro zvuk
        if (!isChasing)
        {
            PlayAggroSound();
            isChasing = true;
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
            StopMoveSound(); // Zastavit zvuk, pokud je knockback/vypnuto
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
            }

            targetPosition = player.position;
        }
        else
        {
            // HRÁČ UTEKL - VRACÍME SE
            isChasing = false;
            currentDetectionRange = aggroRange;

            if (distanceToHome <= stopDistance)
            {
                agent.ResetPath();
                SetAnimator(Vector2.zero);
                StopMoveSound(); // Jsme doma, ticho
                return;
            }

            targetPosition = homePosition;
        }

        // --- Aplikace pohybu ---
        if (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            agent.SetDestination(targetPosition);
            UpdateMoveSound(true); // Hýbeme se -> zapnout zvuk
        }
        else
        {
            agent.ResetPath();
            UpdateMoveSound(false); // Nehýbeme se -> vypnout zvuk
        }

        // Animace
        Vector2 velocity2D = new Vector2(agent.velocity.x, agent.velocity.y);
        SetAnimator(velocity2D);
    }

    // --- FMOD POMOCNÉ METODY ---

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
        if (!aggroSound.IsNull)
        {
            RuntimeManager.PlayOneShot(aggroSound, transform.position);
        }
    }

    // --- KONEC FMOD ---

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