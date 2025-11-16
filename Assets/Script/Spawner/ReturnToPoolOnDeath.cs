using UnityEngine;
using System;

public class ReturnToPoolOnDeath : MonoBehaviour
{
    private ObjectPool pool;
    private EnemyHealth health;
    private Action onReturned;

    private bool subscribed = false;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        if (health != null && !subscribed)
        {
            health.OnDeath += HandleDeath;
            subscribed = true;
        }
    }

    void OnDisable()
    {
        if (health != null && subscribed)
        {
            health.OnDeath -= HandleDeath;
            subscribed = false;
        }
    }

    public void Init(ObjectPool pool, Action onReturned)
    {
        this.pool = pool;
        this.onReturned = onReturned;
    }

    private void HandleDeath()
    {
        // enemy je mrtvý → vrať do poolu
        ForceReturn();
    }

    public void ForceReturn()
    {
        if (pool == null)
        {
            Debug.LogWarning($"ReturnToPoolOnDeath na {name}: pool je null, jen vypínám objekt.");
            gameObject.SetActive(false);
            return;
        }

        // případně vypnutí AI atd. – tady jen pro jistotu:
        // foreach (var comp in GetComponents<MonoBehaviour>())
        //     if (comp != this) comp.enabled = false;

        pool.Return(gameObject);
        onReturned?.Invoke();
    }
}
