using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Nastavení")]
    public float knockbackForce = 10f;
    public float knockbackTime = 0.2f;

    private Rigidbody2D rb;
    private NavMeshAgent agent;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
    }

    // 🔥 TOTO JE NEJDŮLEŽITĚJŠÍ ČÁST PRO OBJECT POOL 🔥
    void OnEnable()
    {
        // 1. Zastavíme jakýkoliv předchozí knockback (pokud běžel při smrti)
        StopAllCoroutines();

        // 2. Resetujeme fyziku zpět na Kinematic (aby enemy nepadal a NavMesh s ním mohl hýbat)
        if (rb != null)
        {
            rb.isKinematic = true;      // Vracíme do režimu pro NavMesh
            rb.velocity = Vector2.zero; // Zastavíme setrvačnost
        }

        // 3. Zapneme Agenta (pokud byl vypnutý kvůli odhození)
        if (agent != null)
        {
            agent.enabled = true;
            // Poznámka: ResetPath tu volat nemusíš, to už dělá tvůj EnemyController v jeho OnEnable
        }
    }

    public void PlayKnockback()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Pokud dostane ránu znovu během letu, resetujeme rutinu
            StopCoroutine("KnockbackRoutine");
            StartCoroutine(KnockbackRoutine(player.transform));
        }
    }

    IEnumerator KnockbackRoutine(Transform playerTransform)
    {
        // 1. Vypneme Agenta (aby se nepral s fyzikou)
        if (agent != null) agent.enabled = false;

        // 2. Zapneme fyziku (Dynamic), abychom mohli použít sílu
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;

            // Směr od hráče k nepříteli
            Vector2 direction = (transform.position - playerTransform.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }

        // 3. Čekáme, až odletí
        yield return new WaitForSeconds(knockbackTime);

        // 4. ÚKLID: Zastavíme ho a vrátíme do režimu Kinematic
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Zpět na Kinematic
        }

        // 5. Vrátíme kontrolu NavMeshAgentovi (pokud enemy mezitím neumřel)
        if (agent != null && gameObject.activeInHierarchy)
        {
            agent.enabled = true;
            // Hack: Někdy potřebuje agent chvilku na vzpamatování, aby se přichytil k NavMeshi
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }
}