using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button button;

    // Odkaz na data tohoto slotu
    public CraftingRecipe recipeData { get; private set; }

    private CraftingManager manager;

    public void Setup(CraftingRecipe recipe, CraftingManager mgr)
    {
        recipeData = recipe;
        manager = mgr;

        if (recipe.icon != null) iconImage.sprite = recipe.icon;
        if (nameText != null) nameText.text = recipe.recipeName;

        // Kliknutí na tlaèítko vybere tento recept v Manageru
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.SelectRecipe(this));
    }
}