using UnityEngine;
using System;
using System.Collections;

public class ReturnToPoolBoss : MonoBehaviour
{
    [Header("Nastavení")]
    public float delayBeforeReturn = 3.0f; // Èas na animaci smrti (napø. 3 sekundy)

    private ObjectPool pool;
    private BossHealth health;
    private Action onReturned;

    private bool subscribed = false;

    void Awake()
    {
        health = GetComponent<BossHealth>();
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
        // Spustíme odpoèet, aby Boss nezmizel okamžitì
        // (Bìhem této doby mùže hrát animace smrti)
        StartCoroutine(WaitAndReturn());
    }

    private IEnumerator WaitAndReturn()
    {
        Debug.Log("Boss umírá... èekám na animaci.");

        // Tady èekáme nastavený èas (napø. 3 vteøiny)
        yield return new WaitForSeconds(delayBeforeReturn);

        ForceReturn();
    }

    public void ForceReturn()
    {
        if (pool == null)
        {
            gameObject.SetActive(false);
            return;
        }

        pool.Return(gameObject);
        onReturned?.Invoke();
    }
}