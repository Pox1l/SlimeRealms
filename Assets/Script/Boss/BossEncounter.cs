using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("Nastavení Bosse")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float startDelay = 0.5f;

    [Header("Propojení")]
    public PixelCameraZoomer cameraZoomer;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 1. Vždy oddálit kameru při vstupu (i když se vrátíš později)
        if (cameraZoomer != null)
        {
            cameraZoomer.ZoomToCombat();
        }

        // 2. Pokud boss ještě nebyl, spustíme spawn sekvenci
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(SpawnSequence());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 3. Při odchodu vrátit kameru do normálu
        if (cameraZoomer != null)
        {
            cameraZoomer.ZoomToNormal();
        }
    }

    IEnumerator SpawnSequence()
    {
        // Čekáme jen na spawn bosse, kamera už jede hned po vstupu
        yield return new WaitForSeconds(startDelay);

        if (bossPrefab != null && spawnPoint != null)
        {
            Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Boss spawned!");
        }

        // ❌ Tady jsem smazal vypnutí collideru, aby fungoval Exit
    }
}