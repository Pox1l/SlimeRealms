using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop prefaby")]
    public GameObject essencePrefab;
    public int essenceAmount = 1;

    public GameObject energyPrefab;
    public int energyAmount = 0;

    public void DropLoot()
    {
        if (essencePrefab != null)
        {
            for (int i = 0; i < essenceAmount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.3f;
                Instantiate(essencePrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        if (energyPrefab != null)
        {
            for (int i = 0; i < energyAmount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.3f;
                Instantiate(energyPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }
    }
}
