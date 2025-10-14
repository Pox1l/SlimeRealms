using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float stopDistance = 0.1f; // 🟢 tolerance pro zastavení u cíle

    private Vector2 startPosition;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position; // uloží pozici jako Vector2
        FindPlayer();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToStart = Vector2.Distance(transform.position, startPosition);

        if (distanceToPlayer <= aggroRange)
        {
            // směr k hráči
            moveDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // návrat na start, ale zastaví se když je dost blízko
            if (distanceToStart > stopDistance)
            {
                moveDirection = (startPosition - (Vector2)transform.position).normalized;
            }
            else
            {
                moveDirection = Vector2.zero; // 🟢 zastaví pohyb
            }
        }

        SetAnimator(moveDirection);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
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
