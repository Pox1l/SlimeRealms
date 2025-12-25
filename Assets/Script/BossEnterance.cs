using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BossEntrance : MonoBehaviour
{
    [System.Serializable]
    public struct EntranceRequirement
    {
        public ItemSO itemSO;
        public int requiredAmount;
    }

    [Header("Propojení")]
    public BossEncounter bossEncounter;

    [Header("Barikáda")]
    public GameObject barrierObject;
    public Collider2D[] collidersToDisable;

    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.E;
    public List<EntranceRequirement> requirements = new List<EntranceRequirement>();
    public InventoryManager inventoryManager;

    [Header("UI Hinty")]
    public GameObject pressEHint;
    public GameObject requirementsPanel;
    public Transform requirementsParent;
    public GameObject requirementPrefab;

    private List<GameObject> uiPool = new List<GameObject>(); // Pool pro UI prvky
    private bool playerInRange = false;
    private bool isActivated = false;

    private void Awake()
    {
        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (barrierObject == null) barrierObject = gameObject;

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();

        // Předvytvoření UI (volitelné, ale pomůže)
        PrepareUIPool();
    }

    private void Update()
    {
        if (!playerInRange || isActivated) return;
        if (Input.GetKeyDown(interactKey)) TryPayAndOpen();
    }

    private void TryPayAndOpen()
    {
        if (!HasAllRequirements()) return;

        foreach (var req in requirements)
        {
            if (req.itemSO != null) inventoryManager.RemoveItem(req.itemSO, req.requiredAmount);
        }

        OpenBarrier();
    }

    private void OpenBarrier()
    {
        isActivated = true;
        if (requirementsPanel != null) requirementsPanel.SetActive(false);
        if (pressEHint != null) pressEHint.SetActive(false);
        if (barrierObject != null) barrierObject.SetActive(false);
        foreach (var col in collidersToDisable) if (col != null) col.enabled = false;
        if (bossEncounter != null) bossEncounter.PrepareBoss();
    }

    public void ResetBarrier()
    {
        isActivated = false;
        if (barrierObject != null) barrierObject.SetActive(true);
        foreach (var col in collidersToDisable) if (col != null) col.enabled = true;
    }

    private bool HasAllRequirements()
    {
        foreach (var req in requirements)
        {
            if (req.itemSO == null) continue;
            if (inventoryManager.GetTotalItemCount(req.itemSO) < req.requiredAmount) return false;
        }
        return true;
    }

    private void PrepareUIPool()
    {
        // Skryjeme stávající děti, pokud nějaké jsou
        foreach (Transform child in requirementsParent) child.gameObject.SetActive(false);
    }

    private void UpdateRequirementsUI()
    {
        if (requirementsPanel == null) return;
        requirementsPanel.SetActive(true);

        // Deaktivujeme všechny stávající řádky v poolu
        for (int i = 0; i < uiPool.Count; i++) uiPool[i].SetActive(false);

        for (int i = 0; i < requirements.Count; i++)
        {
            var req = requirements[i];
            if (req.itemSO == null) continue;

            GameObject row;
            if (i < uiPool.Count)
            {
                row = uiPool[i];
                row.SetActive(true);
            }
            else
            {
                row = Instantiate(requirementPrefab, requirementsParent);
                uiPool.Add(row);
            }

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);
            row.transform.Find("Icon").GetComponent<Image>().sprite = req.itemSO.icon;
            var txt = row.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            txt.text = $"{owned} / {req.requiredAmount}";
            txt.color = (owned < req.requiredAmount) ? Color.red : Color.green;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (!isActivated)
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