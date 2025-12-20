using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    [Header("List Settings (Left Side)")]
    public Transform scrollContentContainer; // 🔥 Sem přetáhni 'Content' objekt ze ScrollView
    public GameObject slotPrefab;            // Prefab tlačítka receptu
    public List<CraftingRecipe> allRecipes;  // Sem naházej recepty v Inspectoru

    [Header("Details Panel (Right Side)")]
    // ZDE JSEM ODEBRAL recipeNameText a resultIcon
    public Transform requirementsContainer;  // Grid pro suroviny
    public GameObject requirementPrefab;     // Prefab ikony suroviny

    [Header("Action")]
    public Button craftButton;
    public TextMeshProUGUI craftButtonText;

    private CraftingSlot selectedSlot;

    private void Start()
    {
        GenerateRecipeList();

        // Na začátku vybereme první recept, pokud existuje
        if (scrollContentContainer.childCount > 0)
        {
            // Musíme získat komponentu z prvního dítěte
            CraftingSlot firstSlot = scrollContentContainer.GetChild(0).GetComponent<CraftingSlot>();
            if (firstSlot != null) SelectRecipe(firstSlot);
        }
        else
        {
            UpdateDetailsUI(); // Vyčistí UI
        }
    }

    // --- 1. Generování seznamu (Scroll) ---
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

    // --- 2. Výběr receptu ---
    public void SelectRecipe(CraftingSlot slot)
    {
        selectedSlot = slot;
        UpdateDetailsUI();
    }

    // --- 3. Update pravého panelu ---
    void UpdateDetailsUI()
    {
        // 1. Pojistka: Pokud nemáme kontejner pro suroviny
        if (requirementsContainer == null)
        {
            Debug.LogError("Chybí reference na 'Requirements Container' v CraftingManageru!");
            return;
        }

        // Vyčistit grid (staré suroviny)
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);

        // 2. Pojistka: Pokud není vybraný slot
        if (selectedSlot == null || selectedSlot.recipeData == null)
        {
            if (craftButton != null) craftButton.interactable = false;
            return;
        }

        CraftingRecipe recipe = selectedSlot.recipeData;
        bool canAfford = true;

        // 3. Výpis surovin
        if (recipe.ingredients != null)
        {
            foreach (var req in recipe.ingredients)
            {
                // Kontrola dat
                if (req.item == null) continue;

                GameObject obj = Instantiate(requirementPrefab, requirementsContainer);

                // Získání počtu itemů (s pojistkou pro InventoryManager)
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

        // 4. Nastavení tlačítka
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

    // --- 4. Samotný Crafting (Musí být vně UpdateDetailsUI) ---
    void TryCraft()
    {
        if (selectedSlot == null || InventoryManager.Instance == null) return;
        CraftingRecipe recipe = selectedSlot.recipeData;

        // 1. Znovu kontrola surovin
        foreach (var req in recipe.ingredients)
        {
            if (InventoryManager.Instance.GetTotalItemCount(req.item) < req.amount) return;
        }

        // 2. Odečíst suroviny
        foreach (var req in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(req.item, req.amount);
        }

        // 3. Přidat výsledek
        InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultAmount);

        // 4. Aktualizovat UI
        UpdateDetailsUI();
        Debug.Log($"Crafted: {recipe.recipeName}");
    }
}