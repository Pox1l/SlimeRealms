using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop prefaby")]
    public GameObject essencePrefab;
    public int essenceAmount = 1;
    [Range(0, 100)] public float essenceChance = 100f; 

    public GameObject energyPrefab;
    public int energyAmount = 0;
    [Range(0, 100)] public float energyChance = 50f; 

    public void DropLoot()
    {
        // Kontrola šance pro Essence
        if (essencePrefab != null && Random.Range(0f, 100f) <= essenceChance)
        {
            for (int i = 0; i < essenceAmount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                Instantiate(essencePrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        // Kontrola šance pro Energy
        if (energyPrefab != null && Random.Range(0f, 100f) <= energyChance)
        {
            for (int i = 0; i < energyAmount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                Instantiate(energyPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }
    }
}