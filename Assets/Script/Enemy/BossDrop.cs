using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DropItem
{
    public GameObject prefab;
    public int amount = 1;
    [Range(0, 100)] public float chance = 100f;
}

public class BossDrop : MonoBehaviour
{
    [Header("Seznam všech možných dropù")]
    public List<DropItem> drops = new List<DropItem>();

    public void DropLoot()
    {
        foreach (DropItem item in drops)
        {
            if (item.prefab != null && Random.Range(0f, 100f) <= item.chance)
            {
                for (int i = 0; i < item.amount; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * 0.5f;
                    Instantiate(item.prefab, (Vector2)transform.position + offset, Quaternion.identity);
                }
            }
        }
    }
}