using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BossEntrance : MonoBehaviour
{
    // Definice požadavku přímo ve skriptu pro maximální univerzálnost
    [System.Serializable]
    public struct EntranceRequirement
    {
        public ItemSO itemSO; 
        public int requiredAmount;
    }

    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Požadované itemy")]
    public List<EntranceRequirement> requirements = new List<EntranceRequirement>();

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

        if (barrierObject == null) barrierObject = gameObject;
        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();
    }

    private void Start()
    {
        if (pressEHint != null) pressEHint.SetActive(false);
        if (requirementsPanel != null) requirementsPanel.SetActive(false);
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
        if (inventoryManager == null) return;

        UpdateRequirementsUI();

        if (!HasAllRequirements())
        {
            Debug.Log("Hráč nemá potřebné itemy.");
            return;
        }

        // Odebrání itemů z tvého inventáře
        foreach (var req in requirements)
        {
            if (req.itemSO != null && req.requiredAmount > 0)
                inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);
        }

        OpenGate();
    }

    private bool HasAllRequirements()
    {
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var req in requirements)
        {
            if (req.itemSO == null || req.requiredAmount <= 0) continue;

            // GetTotalItemCount musí existovat ve tvém InventoryManageru
            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            if (owned < req.requiredAmount) return false;
        }
        return true;
    }

    private void UpdateRequirementsUI()
    {
        if (requirementsPanel != null) requirementsPanel.SetActive(true);
        if (requirementsParent == null || requirementPrefab == null) return;

        foreach (Transform child in requirementsParent) Destroy(child.gameObject);

        foreach (var req in requirements)
        {
            if (req.itemSO == null || req.requiredAmount <= 0) continue;

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            GameObject row = Instantiate(requirementPrefab, requirementsParent);

            Transform iconTransform = row.transform.Find("Icon");
            if (iconTransform != null)
                iconTransform.GetComponent<Image>().sprite = req.itemSO.icon;

            Transform textTransform = row.transform.Find("Text");
            if (textTransform != null)
            {
                var tmpText = textTransform.GetComponent<TextMeshProUGUI>();
                tmpText.text = $"{owned} / {req.requiredAmount}";

                if (owned < req.requiredAmount)
                    tmpText.color = Color.red;
                else
                    tmpText.color = Color.green;
            }
        }
    }

    private void OpenGate()
    {
        isOpened = true;
        if (requirementsPanel != null) requirementsPanel.SetActive(false);
        if (pressEHint != null) pressEHint.SetActive(false);

        foreach (var col in collidersToDisable)
        {
            if (col != null) col.enabled = false;
        }

        if (barrierObject != null) barrierObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (!isOpened)
        {
            if (pressEHint != null) pressEHint.SetActive(true);
            UpdateRequirementsUI();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (pressEHint != null) pressEHint.SetActive(false);
        if (requirementsPanel != null) requirementsPanel.SetActive(false);
    }
}