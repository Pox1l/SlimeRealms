using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("Nastavení")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float startDelay = 0.5f;

    // Defaultně TRUE = boss je "jakože poražen/nedostupný", dokud nezaplatíš u barikády
    public bool bossDefeated = true;

    [Header("Propojení")]
    public PixelCameraZoomer cameraZoomer;
    public BossEntrance entranceScript; // Odkaz na barikádu

    private ObjectPool pool;
    private GameObject activeBoss;
    private Coroutine spawnCoroutine;
    private bool playerInside = false;

    void Awake()
    {
        pool = new ObjectPool(bossPrefab, 1, transform);
    }

    // Volá BossEntrance po odstranění barikády
    public void PrepareBoss()
    {
        bossDefeated = false; // Boss je připraven
        // Pokud už hráč stojí v aréně (např. barikáda byla uvnitř), rovnou spawnuj
        if (playerInside && activeBoss == null)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnSequence());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (cameraZoomer != null) cameraZoomer.ZoomToCombat();

        // 👁️ FIX: Získání reference na UI
        BossHealthUI ui = BossHealthUI.Instance;
        // Pokud je instance null (protože je objekt vypnutý), najdeme ho ručně (true = hledat i neaktivní)
        if (ui == null) ui = FindObjectOfType<BossHealthUI>(true);

        // Zobrazíme UI, jen pokud boss není mrtvý a UI jsme našli
        if (!bossDefeated && ui != null)
        {
            ui.ShowUI();
            // Pro jistotu nastavíme Singleton, kdyby nebyl nastaven
            if (BossHealthUI.Instance == null) BossHealthUI.Instance = ui;
        }

        // Spawn bosse
        if (!bossDefeated && activeBoss == null)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnSequence());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        // 👁️ SCHOVÁNÍ UI OKAMŽITĚ PO ODCHODU
        // Zde taky raději zkontrolujeme, zda máme instanci, případně ji dohledáme
        BossHealthUI ui = BossHealthUI.Instance;
        if (ui == null) ui = FindObjectOfType<BossHealthUI>(true);

        if (ui != null)
        {
            ui.HideUI();
        }

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();

        // Despawn bosse, pokud jsi utekl
        if (activeBoss != null)
        {
            DespawnBoss();
        }
    }

    public void SetBossDefeated()
    {
        bossDefeated = true; // Zámek

        // Schováme UI, protože boss je tuhý
        BossHealthUI ui = BossHealthUI.Instance;
        if (ui == null) ui = FindObjectOfType<BossHealthUI>(true);
        if (ui != null) ui.HideUI();

        if (cameraZoomer != null) cameraZoomer.ZoomToNormal();

        // 🔄 Resetujeme barikádu
        if (entranceScript != null) entranceScript.ResetBarrier();
    }

    // --- Spawn logika ---
    IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(startDelay);
        if (playerInside && activeBoss == null && !bossDefeated) SpawnBoss();
    }

    void SpawnBoss()
    {
        activeBoss = pool.Get();

        // Pozice a Domov
        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        if (activeBoss.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) agent.Warp(pos);
        else activeBoss.transform.position = pos;

        if (activeBoss.TryGetComponent(out EnemyController ctrl)) ctrl.SetHomePosition(pos);

        // Inicializace návratu do poolu
        if (activeBoss.TryGetComponent(out ReturnToPoolBoss ret)) ret.Init(pool, () => activeBoss = null);
    }

    void DespawnBoss()
    {
        if (activeBoss != null && activeBoss.activeSelf)
        {
            if (activeBoss.TryGetComponent(out ReturnToPoolBoss ret)) ret.ForceReturn();
            else activeBoss.SetActive(false);
        }
    }
}