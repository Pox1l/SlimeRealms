using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float stopDistance = 0.1f; // tolerance pro zastavení u cíle

    // "domov" enemáka – kam se vrací, když nevidí hráče
    private Vector3 homePosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Důležité pro 2D (NavMeshPlus)
        agent.updateRotation = false; // neotáčí objekt kolem Z
        agent.updateUpAxis = false;   // ignoruje osu Y jako "nahoru"
        agent.speed = moveSpeed;

        FindPlayer();
    }

    void OnEnable()
    {
        if (agent != null)
            agent.ResetPath();

        // pojistka: pokud by spawner náhodou nezavolal SetHomePosition,
        // tak aspoň vezmeme aktuální pozici
        if (homePosition == Vector3.zero)
            homePosition = transform.position;
    }

    void OnDisable()
    {
        if (agent != null)
            agent.ResetPath();
    }

    /// <summary>
    /// Nastaví domovskou pozici enemáka (volá spawner po spawnutí).
    /// </summary>
    public void SetHomePosition(Vector3 position)
    {
        homePosition = position;
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToHome = Vector2.Distance(transform.position, homePosition);

        Vector3 targetPosition;

        if (distanceToPlayer <= aggroRange)
        {
            // Jdeme za hráčem
            targetPosition = player.position;
        }
        else
        {
            // Vracíme se domů
            if (distanceToHome <= stopDistance)
            {
                agent.ResetPath();
                SetAnimator(Vector2.zero);
                return;
            }

            targetPosition = homePosition;
        }

        if (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.ResetPath();
        }

        // Animace podle rychlosti agenta
        Vector2 velocity2D = new Vector2(agent.velocity.x, agent.velocity.y);
        SetAnimator(velocity2D);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
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
