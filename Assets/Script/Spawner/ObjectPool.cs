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
        if (free.Count == 0)
        {
            GameObject extra = Object.Instantiate(prefab, parent);
            extra.SetActive(false);
            free.Push(extra);
        }

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
