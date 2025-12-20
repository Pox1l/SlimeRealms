using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;       // Název (napø. "Heal Potion I")
    public ItemSO resultItem;       // Co dostaneš (Item)
    public int resultAmount = 1;    // Kolik toho dostaneš
    public Sprite icon;             // Ikonka pro seznam vlevo

    [Header("Costs")]
    public List<Requirement> ingredients; // Seznam surovin (stejný struct jako u Skillù)
}