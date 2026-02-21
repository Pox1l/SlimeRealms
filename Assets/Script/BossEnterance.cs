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
        }

        if (barrierObject == null) barrierObject = gameObject;

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();

        PrepareUIPool();
    }

    // 🔥 OPRAVA 1: Přesunuli jsme hledání InventoryManageru do Start().
    // Ve funkci Awake() ještě nemusel InventoryManager.Instance vůbec existovat!
    private void Start()
    {
        if (inventoryManager == null && InventoryManager.Instance != null)
        {
            inventoryManager = InventoryManager.Instance;
        }
    }

    private void FindUIReferences()
    {
        Transform foundPanel = FindDeepChild(WorldCanvas.transform, "ReqBossPanel");
        if (foundPanel != null)
        {
            requirementsPanel = foundPanel.gameObject;

            // Zkontroluj, jestli se tvůj objekt fakt jmenuje "ReqCoinrtainer" a nemáš tam překlep!
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
        // 🔥 POJISTKA PROTI CHYBĚ: Kdyby se InventoryManager nenačetl
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null) return false;

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

        uiPool.Clear();
        // 🔥 OPRAVA 2: Správné naplnění Poolu. Objekty, které už v panelu jsou z Editoru, 
        // se teď přidají do seznamu a použijí se, místo aby se vytvořily nové duplikáty.
        foreach (Transform child in requirementsParent)
        {
            child.gameObject.SetActive(false);
            uiPool.Add(child.gameObject);
        }
    }

    private void UpdateRequirementsUI()
    {
        if (requirementsPanel == null) return;
        requirementsPanel.SetActive(true);

        // 🔥 POJISTKA PROTI CHYBĚ
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("BossEntrance: Nelze načíst data, chybí InventoryManager!");
            return;
        }

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

            // Tady to předtím padalo a zastavilo skript, protože se nenašel InventoryManager
            int owned = inventoryManager.GetTotalItemCount(req.itemSO);

            Transform iconTrans = row.transform.Find("Icon");
            Transform textTrans = row.transform.Find("Text");

            if (iconTrans != null) iconTrans.GetComponent<Image>().sprite = req.itemSO.icon;
            if (textTrans != null)
            {
                var txt = textTrans.GetComponent<TextMeshProUGUI>();
                txt.text = $"{owned} / {req.requiredAmount}";
                txt.color = (owned < req.requiredAmount) ? Color.red : Color.green;
            }
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