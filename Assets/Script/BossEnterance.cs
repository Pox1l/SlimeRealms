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
    public GameObject WorldCanvas;
    public GameObject pressEHint;
    public GameObject requirementsPanel;
    public Transform requirementsParent;
    public GameObject requirementPrefab;

    private List<GameObject> uiPool = new List<GameObject>();
    private bool playerInRange = false;
    private bool isActivated = false;

    private void Awake()
    {
        if (WorldCanvas == null)
        {
            WorldCanvas = GameObject.FindGameObjectWithTag("WorldUI");
        }

        if (requirementsPanel == null && WorldCanvas != null)
        {
            FindUIReferences();
        }
        else if (requirementsPanel == null && WorldCanvas == null)
        {
            Debug.LogError("BossEntrance: WorldCanvas chybí (není přiřazen ani nalezen tagem 'WorldUI')");
        }

        // 🔍 ÚPRAVA: Hledání hintu "E" přímo pod tímto objektem podle tvého screenu
        if (pressEHint == null)
        {
            Transform foundHint = transform.Find("E");
            if (foundHint != null)
            {
                pressEHint = foundHint.gameObject;
            }
        }

        if (bossEncounter == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("BossEncounter");
            if (bossObj != null)
            {
                bossEncounter = bossObj.GetComponent<BossEncounter>();
            }
            else
            {
                Debug.LogWarning("BossEntrance: Objekt s tagem 'BossEncounter' nebyl nalezen!");
            }
        }

        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (barrierObject == null) barrierObject = gameObject;

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();

        PrepareUIPool();
    }

    private void FindUIReferences()
    {
        Transform foundPanel = FindDeepChild(WorldCanvas.transform, "ReqBossPanel");
        if (foundPanel != null)
        {
            requirementsPanel = foundPanel.gameObject;

            Transform foundParent = FindDeepChild(foundPanel, "ReqCoinrtainer");
            if (foundParent != null)
            {
                requirementsParent = foundParent;
            }
            else
            {
                requirementsParent = foundPanel;
            }
        }
        else
        {
            Debug.LogError($"BossEntrance: Máme Canvas '{WorldCanvas.name}', ale uvnitř není 'ReqBossPanel'!");
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
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
        if (requirementsParent == null) return;
        foreach (Transform child in requirementsParent) child.gameObject.SetActive(false);
    }

    private void UpdateRequirementsUI()
    {
        if (requirementsPanel == null) return;
        requirementsPanel.SetActive(true);

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
                if (requirementsParent == null) return;
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