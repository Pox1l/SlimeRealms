using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemSO> allItems = new List<ItemSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Vytrhnout, aby mohl být DontDestroy
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemSO GetItemByID(int itemID)
    {
        return allItems.Find(item => item.itemID == itemID);
    }
}