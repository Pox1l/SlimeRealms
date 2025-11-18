using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BossEntrance : MonoBehaviour
{
    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Požadované itemy (stejné jako u krystalu)")]
    public List<CrystalUIController.Requirement> requirements = new List<CrystalUIController.Requirement>();

    [Header("Inventory")]
    public InventoryManager inventoryManager;

    [Header("Bariéra")]
    public GameObject barrierObject;
    public Collider2D[] collidersToDisable;

    [Header("UI / Hint")]
    public GameObject pressEHint;
    public GameObject requirementsPanel;
    public Transform requirementsParent;
    public GameObject requirementPrefab;

    private bool playerInRange = false;
    private bool isOpened = false;

    private void Awake()
    {
        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (barrierObject == null)
            barrierObject = gameObject;

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();
    }

    private void Start()
    {
        if (pressEHint != null)
            pressEHint.SetActive(false);

        if (requirementsPanel != null)
            requirementsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || isOpened) return;

        if (Input.GetKeyDown(interactKey))
        {
            TryOpenGate();
        }
    }

    private void TryOpenGate()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[BossEntrance] Chybí InventoryManager!");
            return;
        }

        // 1) čistá kontrola itemů
        bool hasAll = HasAllRequirements();

        // 2) UI jen zobrazí aktuální stav
        UpdateRequirementsUI();

        if (!hasAll)
        {
            // hráč nemá vše → jen vidí UI, brána se neotevře
            return;
        }

        // 3) má vše → odeber itemy
        foreach (var req in requirements)
        {
            if (req.itemSO == null || req.requiredAmount <= 0) continue;
            inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);
        }

        OpenGate();
    }

    /// <summary>
    /// Vrátí true, když má hráč všechny požadované itemy.
    /// Nezávislé na UI.
    /// </summary>
    private bool HasAllRequirements()
    {
        if (requirements == null || requirements.Count == 0)
        {
            // pokud chceš, aby bez nastavených požadavků šlo OTEVŘÍT, dej tady "return true;"
            return false;
        }

        foreach (var req in requirements)
        {
            if (req.itemSO == null || req.requiredAmount <= 0) continue;

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Postaví UI seznam požadavků (stejně jako krystal).
    /// </summary>
    private void UpdateRequirementsUI()
    {
        if (requirementsPanel != null)
            requirementsPanel.SetActive(true);

        if (requirementsParent == null || requirementPrefab == null)
            return;

        foreach (Transform child in requirementsParent)
            Destroy(child.gameObject);

        foreach (var req in requirements)
        {
            if (req.itemSO == null || req.requiredAmount <= 0) continue;

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);

            GameObject row = Instantiate(requirementPrefab, requirementsParent);
            row.transform.Find("Icon").GetComponent<Image>().sprite = req.itemSO.icon;

            var text = row.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            text.text = $"{owned}/{req.requiredAmount}x";
        }
    }

    private void OpenGate()
    {
        isOpened = true;

        if (requirementsPanel != null)
            requirementsPanel.SetActive(false);

        if (pressEHint != null)
            pressEHint.SetActive(false);

        foreach (var col in collidersToDisable)
        {
            if (col != null) col.enabled = false;
        }

        if (barrierObject != null)
            barrierObject.SetActive(false);

        Debug.Log("[BossEntrance] Bariéra otevřena.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (!isOpened && pressEHint != null)
            pressEHint.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (pressEHint != null)
            pressEHint.SetActive(false);

        if (requirementsPanel != null)
            requirementsPanel.SetActive(false);
    }
}
