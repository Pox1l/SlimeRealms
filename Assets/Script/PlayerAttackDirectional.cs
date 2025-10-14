using UnityEngine;

public class PlayerAttackDirectional : MonoBehaviour
{
    [Header("Damage & Cadence")]
    public int damage = 10;
    public float attackRate = 3f; // útoky za sekundu
    private float nextAttackTime;

    [Header("Hitbox (před hráčem)")]
    public Vector2 boxSize = new Vector2(1.6f, 0.8f); // šířka x výška zásahu
    public float boxDistance = 1.0f;                  // jak daleko před hráčem


    public GameObject slashPrefab;
    public float slashDuration = 0.2f;


    [Header("Cílení")]
    public LayerMask enemyLayers;                     // nastav třeba na "Enemy"
    public Camera cam;                                // nech prázdné -> Camera.main

    // Poslední vypočtená data pro gizma
    private Vector2 lastCenter;
    private float lastAngleDeg;

    void Update()
    {
        if (Time.time < nextAttackTime) return;

        if (Input.GetMouseButtonDown(0))
        {
            DoAttack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void DoAttack()
{
    var cameraToUse = cam != null ? cam : Camera.main;

    if (cameraToUse == null)
    {
        Debug.LogWarning("PlayerAttackDirectional: Nenalezena kamera. Přiřaď 'cam' nebo použij Camera.main.");
        return;
    }

    Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
    mouseWorld.z = 0f;

    Vector2 playerPos = transform.position;
    Vector2 aimDir = (Vector2)(mouseWorld - (Vector3)playerPos);
    if (aimDir.sqrMagnitude < 0.0001f) aimDir = Vector2.right; // fallback, kdyby byl kurzor "v tobě"
    aimDir.Normalize();

    float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
    Vector2 center = playerPos + aimDir * boxDistance;

    // Spawn vizuálu slashe (už víme center + úhel)
    GameObject slash = Instantiate(slashPrefab, center, Quaternion.Euler(0, 0, angleDeg -90));
    Destroy(slash, slashDuration);

    // Ulož pro gizma
    lastCenter = center;
    lastAngleDeg = angleDeg;

    // Damage check
    Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angleDeg, enemyLayers);
    for (int i = 0; i < hits.Length; i++)
    {
        if (hits[i] == null) continue;
        if (hits[i].TryGetComponent(out EnemyHealth eh))
        {
            eh.TakeDamage(damage);
        }
    }
}


    void OnDrawGizmos()
    {
        // Náhled hitboxu i v editoru
        Gizmos.color = Color.yellow;

        // Pokud běží hra, kreslíme poslední vypočtený box (odpovídá poslednímu útoku)
        // Mimo Play Mode zkusíme kreslit podle pozice myši proti hlavní kameře.
        Vector2 center = lastCenter;
        float angle = lastAngleDeg;

        if (!Application.isPlaying)
        {
            var cameraToUse = cam != null ? cam : Camera.main;
            if (cameraToUse != null)
            {
                Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                Vector2 playerPos = transform.position;
                Vector2 aimDir = ((Vector2)mouseWorld - playerPos).normalized;
                if (aimDir.sqrMagnitude < 0.0001f) aimDir = Vector2.right;

                angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                center = playerPos + aimDir * boxDistance;
            }
            else
            {
                center = transform.position;
                angle = 0f;
            }
        }

        // Otočený wire-cube
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boxSize.x, boxSize.y, 0));
        Gizmos.matrix = old;

        // Pro lepší orientaci čára z hráče do středu hitboxu
        Gizmos.DrawLine(transform.position, center);
    }
}
