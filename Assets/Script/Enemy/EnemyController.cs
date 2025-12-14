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
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroRange = 5f;   // Kdy si hráče všimne sám
    [SerializeField] private float chaseRange = 15f;  // Jak daleko pronásleduje, když je naštvaný

    private float currentDetectionRange; // Aktuální dosah (mění se)
    private Vector3 homePosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;

        currentDetectionRange = aggroRange; // Začínáme s normálním dosahem

        FindPlayer();
    }

    void OnEnable()
    {
        
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (homePosition == Vector3.zero) homePosition = transform.position;
        currentDetectionRange = aggroRange;
    }

    // Tuto metodu volá EnemyHealth při zásahu
    public void OnHitAggro()
    {
        currentDetectionRange = chaseRange; // Zvětšíme dosah -> naštve se
    }

    public void SetHomePosition(Vector3 position)
    {
        homePosition = position;
    }

    void Update()
    {
        // 🔥 OPRAVA: Pokud je Agent vypnutý (děje se Knockback), nic neděláme a čekáme
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        // --- ZBYTEK TVÉHO PŮVODNÍHO KÓDU ---
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToHome = Vector2.Distance(transform.position, homePosition);

        Vector3 targetPosition;

        // Používáme dynamický 'currentDetectionRange'
        if (distanceToPlayer <= currentDetectionRange)
        {
            // --- Jdeme za hráčem ---
            targetPosition = player.position;
        }
        else
        {
            // --- Hráč utekl moc daleko -> Vracíme se domů ---
            currentDetectionRange = aggroRange;

            if (distanceToHome <= stopDistance)
            {
                agent.ResetPath();
                SetAnimator(Vector2.zero);
                return;
            }

            targetPosition = homePosition;
        }

        // Pohyb
        if (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.ResetPath();
        }

        // Animace
        Vector2 velocity2D = new Vector2(agent.velocity.x, agent.velocity.y);
        SetAnimator(velocity2D);
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
        // Červeně: Kdy si všimne hráče (normálně)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Žlutě: Kam až pronásleduje po hitu
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}