using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPhaseController : MonoBehaviour
{
    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Behavior Settings")]
    [Tooltip("Pokud je true, enemy okamžitì po spawnu ví o hráèi a jde po nìm.")]
    [SerializeField] private bool aggroOnSpawn = true;
    [SerializeField] private float moveSpeed = 2.5f; // Minioni bývají o nìco rychlejší
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Detection (If not Aggro on Spawn)")]
    [SerializeField] private float aggroRange = 10f; // Vìtší range pro arénu
    [SerializeField] private float loseAggroRange = 20f; // Kdy to vzdá (v arénì by to nemìl vzdát nikdy)

    [Header("Audio (FMOD)")]
    public EventReference movementSound;
    public EventReference aggroSound;

    private bool isAggroed = false;
    private EventInstance moveInstance;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;

        FindPlayer();
    }

    void Start()
    {
        // Setup FMOD zvuku
        if (!movementSound.IsNull)
        {
            moveInstance = RuntimeManager.CreateInstance(movementSound);
            RuntimeManager.AttachInstanceToGameObject(moveInstance, gameObject, GetComponent<Rigidbody2D>());
        }
    }

    void OnEnable()
    {
        // Reset pøi vytažení z Poolu
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();

        isAggroed = aggroOnSpawn; // Pokud je true, je naštvaný hned

        if (isAggroed) PlayAggroSound();
    }

    void OnDisable() => StopMoveSound();
    void OnDestroy() { StopMoveSound(); moveInstance.release(); }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            StopMoveSound();
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            StopMoveSound();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- Logika Aggra ---
        if (!isAggroed)
        {
            // Ještì hráèe nevidìl, èeká na vzdálenost
            if (distanceToPlayer <= aggroRange)
            {
                isAggroed = true;
                PlayAggroSound();
            }
            else
            {
                // Idle - stojí a èeká
                SetAnimator(Vector2.zero);
                StopMoveSound();
                return;
            }
        }

        // --- Logika Pronásledování (Hunter Mode) ---
        // Pokud je v arénì, vìtšinou chceme, aby pronásledoval navždy.
        // Ale pro jistotu, kdyby hráè zmizel daleko:
        if (distanceToPlayer > loseAggroRange && !aggroOnSpawn)
        {
            isAggroed = false; // Ztratil zájem (jen pokud nemá perma-aggro)
            agent.ResetPath();
            return;
        }

        // Pohyb k hráèi
        if (distanceToPlayer > stopDistance)
        {
            agent.SetDestination(player.position);
            UpdateMoveSound(true);
        }
        else
        {
            agent.ResetPath(); // Je u hráèe, zastaví
            UpdateMoveSound(false);
        }

        // Animace
        SetAnimator(agent.velocity);
    }

    // --- Audio & Helpers ---

    void UpdateMoveSound(bool isMoving)
    {
        if (moveInstance.isValid())
        {
            moveInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (isMoving && state != PLAYBACK_STATE.PLAYING) moveInstance.start();
            else if (!isMoving && state == PLAYBACK_STATE.PLAYING) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void StopMoveSound()
    {
        if (moveInstance.isValid()) moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void PlayAggroSound()
    {
        if (!aggroSound.IsNull) RuntimeManager.PlayOneShot(aggroSound, transform.position);
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
    }
}