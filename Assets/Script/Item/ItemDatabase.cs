using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    // Seznam všech položek v databázi
    public List<ItemSO> allItems = new List<ItemSO>();

    private void Awake()
    {
        // Singleton pattern pro pøístup k této tøídì
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Metoda pro získání položky podle jejího ID
    public ItemSO GetItemByID(int itemID)
    {
        return allItems.Find(item => item.itemID == itemID);  // Najde první položku s odpovídajícím ID
    }
}