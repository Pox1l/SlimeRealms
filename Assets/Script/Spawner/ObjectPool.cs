using UnityEngine;
using System.Collections.Generic;

public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Stack<GameObject> free = new Stack<GameObject>();

    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            free.Push(obj);
        }
    }

    public GameObject Get()
    {
        // Pokud nemáme žádné volné, vyrobíme nový a rovnou ho vrátíme (nemusíme ho dávat do Stacku)
        if (free.Count == 0)
        {
            GameObject extra = Object.Instantiate(prefab, parent);
            extra.SetActive(true); // Zapneme ho
            return extra;
        }

        // Pokud máme volné, vezmeme ho ze Stacku
        GameObject obj = free.Pop();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        free.Push(obj);
    }
}
