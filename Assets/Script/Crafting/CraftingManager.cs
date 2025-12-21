using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    [Header("List Settings (Left Side)")]
    public Transform scrollContentContainer;
    public GameObject slotPrefab;
    public List<CraftingRecipe> allRecipes;

    public ScrollRect recipeScroll;

    [Header("Details Panel (Right Side)")]
    public Transform requirementsContainer;
    public GameObject requirementPrefab;

    [Header("Action")]
    public Button craftButton;
    public TextMeshProUGUI craftButtonText;

    private CraftingSlot selectedSlot;

    
    private void Start()
    {
        GenerateRecipeList();

        if (scrollContentContainer.childCount > 0)
        {
            CraftingSlot firstSlot = scrollContentContainer.GetChild(0).GetComponent<CraftingSlot>();
            if (firstSlot != null) selectedSlot = firstSlot;
        }

        if (scrollContentContainer.childCount > 0)
        {
            CraftingSlot firstSlot = scrollContentContainer.GetChild(0).GetComponent<CraftingSlot>();
            if (firstSlot != null)
            {
                selectedSlot = firstSlot; 
            }
        }
    }

    
    private void OnEnable()
    {
        UpdateDetailsUI();

        if (recipeScroll != null)
        {
            recipeScroll.verticalNormalizedPosition = 1f; 
        }
    }

    void GenerateRecipeList()
    {
        foreach (Transform child in scrollContentContainer) Destroy(child.gameObject);

        foreach (var recipe in allRecipes)
        {
            GameObject obj = Instantiate(slotPrefab, scrollContentContainer);
            CraftingSlot slot = obj.GetComponent<CraftingSlot>();
            slot.Setup(recipe, this);
        }
    }

    public void SelectRecipe(CraftingSlot slot)
    {
        selectedSlot = slot;
        UpdateDetailsUI();
    }

    
    public void UpdateDetailsUI() 
    {
        
        if (requirementsContainer == null) return;

        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);

        if (selectedSlot == null || selectedSlot.recipeData == null)
        {
            if (craftButton != null) craftButton.interactable = false;
            if (craftButtonText != null) craftButtonText.text = "Select Recipe";
            return;
        }

        CraftingRecipe recipe = selectedSlot.recipeData;
        bool canAfford = true;

        if (recipe.ingredients != null)
        {
            foreach (var req in recipe.ingredients)
            {
                if (req.item == null) continue;

                GameObject obj = Instantiate(requirementPrefab, requirementsContainer);

                
                int playerHas = 0;
                if (InventoryManager.Instance != null)
                {
                    playerHas = InventoryManager.Instance.GetTotalItemCount(req.item);
                }

                var reqUI = obj.GetComponent<RequirementUI>();
                if (reqUI) reqUI.Setup(req.item.icon, playerHas, req.amount);

                
                if (playerHas < req.amount) canAfford = false;
            }
        }

        
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(TryCraft);

            
            craftButton.interactable = canAfford;
        }

        if (craftButtonText != null)
        {
            craftButtonText.text = canAfford ? "Craft" : "Not enough";
        }
    }

    void TryCraft()
    {
        if (selectedSlot == null || InventoryManager.Instance == null) return;
        CraftingRecipe recipe = selectedSlot.recipeData;

        
        foreach (var req in recipe.ingredients)
        {
            if (InventoryManager.Instance.GetTotalItemCount(req.item) < req.amount) return;
        }

        foreach (var req in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(req.item, req.amount);
        }

        InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultAmount);

        UpdateDetailsUI();
        Debug.Log($"Crafted: {recipe.recipeName}");
    }
}