using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class RepairRequirement
{
    public ItemSO item;   // co je potřeba
    public int amount;    // kolik kusů
}

[System.Serializable]
public class RepairPhase
{
    public List<RepairRequirement> requirements = new List<RepairRequirement>();
    public int repairValue = 25; // kolik HP přidá tahle fáze
}

public class CrystalRepair : MonoBehaviour
{
    [Header("Crystal Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Repair Phases")]
    public List<RepairPhase> phases = new List<RepairPhase>();
    private int currentPhase = 0;

    [Header("UI")]
    public Slider crystalSlider;       // slider HP krystalu
    public Canvas sliderCanvas;        // world-space canvas se sliderem

    [Header("Requirements UI")]
    public GameObject requirementPrefab;   // Prefab (Image + TMP_Text)
    public Transform requirementsParent;   // Parent (Vertical/Horizontal Layout Group)
    public Canvas requirementsCanvas;      // Canvas pro požadavky

    private bool playerInRange;

    private void Start()
    {
        currentHealth = 0;
        UpdateUI();

        if (sliderCanvas != null)
            sliderCanvas.enabled = false;

        if (requirementsCanvas != null)
            requirementsCanvas.enabled = false;

        // ✅ Hned si připrav požadavky
        ShowRequirements();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Stiskni F pro opravu krystalu.");

            if (sliderCanvas != null)
                sliderCanvas.enabled = true;

            if (requirementsCanvas != null)
            {
                requirementsCanvas.enabled = true;
                ShowRequirements(); // ✅ rovnou zobraz požadavky
            }
        }
    }


    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TryRepair();
        }
    }

    public void TryRepair()
    {
        if (currentPhase >= phases.Count)
        {
            Debug.Log("Krystal je plně opraven!");
            return;
        }

        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        RepairPhase phase = phases[currentPhase];

        // ✅ Ověřit, že hráč má všechny itemy
        foreach (var req in phase.requirements)
        {
            if (!HasEnoughItems(inv, req.item, req.amount))
            {
                Debug.Log("Chybí materiál pro tuto fázi opravy!");
                return;
            }
        }

        // ✅ Odebrat itemy
        foreach (var req in phase.requirements)
        {
            RemoveItems(inv, req.item, req.amount);
        }

        // ✅ Opravit krystal
        currentHealth = Mathf.Min(currentHealth + phase.repairValue, maxHealth);
        UpdateUI();
        currentPhase++;

        Debug.Log($"Fáze {currentPhase} dokončena! Krystal HP: {currentHealth}");

        ShowRequirements(); // aktualizuj požadavky pro další fázi
    }

    private bool HasEnoughItems(InventoryManager inv, ItemSO item, int needed)
    {
        int total = 0;
        foreach (var slot in inv.itemSlots)
        {
            if (slot.itemData == item)
            {
                total += slot.quantity;
                if (total >= needed) return true;
            }
        }
        return false;
    }

    private void RemoveItems(InventoryManager inv, ItemSO item, int amount)
    {
        int remaining = amount;
        foreach (var slot in inv.itemSlots)
        {
            if (slot.itemData == item)
            {
                int toRemove = Mathf.Min(slot.quantity, remaining);
                slot.RemoveItem(toRemove);
                remaining -= toRemove;
                if (remaining <= 0) break;
            }
        }
    }

    private void UpdateUI()
    {
        if (crystalSlider != null)
        {
            crystalSlider.maxValue = maxHealth;
            crystalSlider.value = currentHealth;
        }
    }

    private void ShowRequirements()
    {
        if (requirementsParent == null || requirementPrefab == null) return;

        // Vyčisti staré
        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        if (currentPhase >= phases.Count) return;

        RepairPhase phase = phases[currentPhase];

        foreach (var req in phase.requirements)
        {
            var go = Instantiate(requirementPrefab, requirementsParent);

            // očekáváme, že prefab má Image + TMP_Text
            var icon = go.GetComponentInChildren<Image>();
            var text = go.GetComponentInChildren<TMP_Text>();

            if (icon != null) icon.sprite = req.item.icon;
            if (text != null) text.text = $"x{req.amount}";
        }
    }

    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;

            if (sliderCanvas != null)
                sliderCanvas.enabled = false;

            if (requirementsCanvas != null)
                requirementsCanvas.enabled = false;
        }
    }
}
