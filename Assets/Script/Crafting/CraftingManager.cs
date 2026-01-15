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
    public Image selectedItemImage;       // PŘIDÁNO: Ikonka nahoře
    public TextMeshProUGUI itemValueText; // PŘIDÁNO: Text pro "Heal: 20 HP"

    public Transform requirementsContainer; // Sem přetáhni 'contPref'
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
            if (firstSlot != null) SelectRecipe(firstSlot); // Opraveno volání pro inicializaci prvního
        }
    }


    private void OnEnable()
    {
        UpdateDetailsUI();
        if (recipeScroll != null) recipeScroll.verticalNormalizedPosition = 1f;
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
        // 1. Vymazat staré suroviny
        if (requirementsContainer != null)
        {
            foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        }

        if (selectedSlot == null || selectedSlot.recipeData == null)
        {
            if (craftButton != null) craftButton.interactable = false;
            if (craftButtonText != null) craftButtonText.text = "Select Recipe";
            if (selectedItemImage != null) selectedItemImage.enabled = false; // Skrýt img
            if (itemValueText != null) itemValueText.text = "";
            return;
        }

        CraftingRecipe recipe = selectedSlot.recipeData;

        // 2. Nastavení obrázku (Velká ikonka nahoře)
        if (selectedItemImage != null)
        {
            selectedItemImage.enabled = true;
            selectedItemImage.sprite = recipe.resultItem.icon;
            selectedItemImage.preserveAspect = true; // Aby se nedeformovala
        }

        // 3. Nastavení Textu (Heal Amount nebo Jméno)
        if (itemValueText != null)
        {
            // Trik: Zkontrolujeme, jestli je resultItem typu HealingItemSO
            if (recipe.resultItem is HealingItemSO healingItem)
            {
                itemValueText.text = $"Heals: <color=#00FF00>{healingItem.healAmount} HP</color>";
            }
            else
            {
                // Pokud to není potion, vypíšeme jen jméno nebo obecný popis
                itemValueText.text = recipe.resultItem.itemName;
            }
        }

        // 4. Vygenerování ingrediencí
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

        // 5. Nastavení tlačítka
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

        // Fix layoutu, pokud používáš ScrollView v requirements
        if (requirementsContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(requirementsContainer.GetComponent<RectTransform>());
    }

    void TryCraft()
    {
        if (selectedSlot == null || InventoryManager.Instance == null) return;
        CraftingRecipe recipe = selectedSlot.recipeData;

        // Kontrola surovin
        foreach (var req in recipe.ingredients)
        {
            if (InventoryManager.Instance.GetTotalItemCount(req.item) < req.amount) return;
        }

        // Odebrání surovin
        foreach (var req in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(req.item, req.amount);
        }

        // Přidání výsledku
        InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultAmount);

        UpdateDetailsUI();
        Debug.Log($"Crafted: {recipe.recipeName}");
    }
}