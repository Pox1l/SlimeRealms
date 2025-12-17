using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TutorialNavigator : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpacing = 1.0f;

    public Transform playerTransform;
    public Transform targetTransform;
    public bool autoUpdate = true;

    // Pokud true, používá NavMesh. Pokud false, kreslí èáru vzdušnou èarou.
    public bool useNavMesh = false;

    private List<GameObject> spawnedArrows = new List<GameObject>();
    private NavMeshPath path;

    void Awake()
    {
        path = new NavMeshPath();
    }

    void Update()
    {
        // 1. Automatické hledání hráèe podle TAGU, pokud chybí
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // 2. Aktualizace cesty
        if (autoUpdate && playerTransform != null && targetTransform != null)
        {
            ShowPath(playerTransform, targetTransform);
        }
    }

    public void ShowPath(Transform player, Transform target)
    {
        // Optimalizace: Smazat staré šipky
        ClearPath();

        bool pathFound = false;

        // Zkusíme NavMesh pouze pokud je to povoleno
        if (useNavMesh)
        {
            pathFound = NavMesh.CalculatePath(player.position, target.position, NavMesh.AllAreas, path);
        }

        if (pathFound && path.status == NavMeshPathStatus.PathComplete)
        {
            // Cesta nalezena pøes NavMesh
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                SpawnArrowsBetween(path.corners[i], path.corners[i + 1]);
            }
        }
        else
        {
            // FALLBACK: Pøímá èára (když není NavMesh nebo je vypnutý)
            SpawnArrowsBetween(player.position, target.position);
        }
    }

    void SpawnArrowsBetween(Vector3 start, Vector3 end)
    {
        // Ignorujeme Z osu pro 2D
        Vector3 start2D = new Vector3(start.x, start.y, 0);
        Vector3 end2D = new Vector3(end.x, end.y, 0);

        float distance = Vector3.Distance(start2D, end2D);

        // Pokud jsme velmi blízko cíle (< 1.5 jednotky), šipky už nekreslíme
        if (distance < 1.5f) return;

        Vector3 direction = (end2D - start2D).normalized;

        // Výpoèet úhlu pro 2D Sprite
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        int count = Mathf.FloorToInt(distance / arrowSpacing);

        // Zaèínáme od 1, aby první šipka nebyla pøímo v hráèi
        for (int i = 1; i <= count; i++)
        {
            Vector3 pos = start2D + direction * (i * arrowSpacing);

            GameObject arrow = Instantiate(arrowPrefab, pos, rotation);
            arrow.transform.SetParent(this.transform); // Uklidí šipky pod tento objekt v hierarchii
            spawnedArrows.Add(arrow);
        }
    }

    public void ClearPath()
    {
        foreach (var arrow in spawnedArrows)
        {
            if (arrow != null) Destroy(arrow);
        }
        spawnedArrows.Clear();
    }
}