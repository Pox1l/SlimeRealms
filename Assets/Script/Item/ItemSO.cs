using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int itemID;   // ⚡ Jednoznačný identifikátor
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 10;
}
