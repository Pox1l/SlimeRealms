using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    void Start()
    {
        // 1. Najde hráèe podle tagu "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 2. Najde spawn point podle tagu "SpawnPoint"
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        // 3. Pøesune hráèe na pozici spawn pointu
        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.LogError("Chybí objekt s tagem 'Player' nebo 'SpawnPoint'!");
        }
    }
}