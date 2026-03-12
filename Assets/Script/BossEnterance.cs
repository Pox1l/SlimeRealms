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

    [Header("Debug")] // PŘIDÁNO: Proměnná pro error text
    public GameObject debugErrorText;

    private List<GameObject> uiPool = new List<GameObject>();
    private bool playerInRange = false;
    private bool isActivated = false;

    private void Awake()
    {
        // PŘIDÁNO: Nalezení debug textu podle tagu
        if (debugErrorText == null)
        {
            debugErrorText = GameObject.FindGameObjectWithTag("debugText");
        }

        // Ujistíme se, že je na startu vypnutý, pokud se ho povedlo najít
        if (debugErrorText != null)
        {
            debugErrorText.SetActive(false);
        }

        // 1. Zajištění WorldCanvasu
        if (WorldCanvas == null)
        {
            WorldCanvas = GameObject.FindGameObjectWithTag("WorldUI");
        }

        // 2. Chytřejší dohledání UI: Hledáme parenta i v případě, že panel už byl přiřazen ručně!
        if (WorldCanvas != null)
        {
            if (requirementsPanel == null)
            {
                Transform foundPanel = FindDeepChild(WorldCanvas.transform, "ReqBossPanel");
                if (foundPanel != null) requirementsPanel = foundPanel.gameObject;
            }

            if (requirementsPanel != null && requirementsParent == null)
            {
                Transform foundParent = FindDeepChild(requirementsPanel.transform, "ReqCoinrtainer");
                requirementsParent = foundParent != null ? foundParent : requirementsPanel.transform;
            }
        }
        else
        {
            Debug.LogError("BossEntrance: WorldCanvas chybí (není přiřazen ani nalezen tagem 'WorldUI')");
            // PŘIDÁNO: Zobrazení chyby, pokud chybí celý Canvas
            if (debugErrorText != null) debugErrorText.SetActive(true);
        }

        // 3. Hint
        if (pressEHint == null)
        {
            Transform foundHint = transform.Find("E");
            if (foundHint != null) pressEHint = foundHint.gameObject;
        }

        // 4. Boss
        if (bossEncounter == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("BossEncounter");
            if (bossObj != null) bossEncounter = bossObj.GetComponent<BossEncounter>();
        }

        if (barrierObject == null) barrierObject = gameObject;
        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = barrierObject.GetComponents<Collider2D>();

        PrepareUIPool();
    }

    private void Start()
    {
        ZkontrolujInventar();
    }

    // NOVÁ METODA: Slouží k dodatečnému nalezení inventáře
    private void ZkontrolujInventar()
    {
        if (inventoryManager == null)
        {
            if (InventoryManager.Instance != null)
            {
                inventoryManager = InventoryManager.Instance;
            }
            else
            {
                // Fallback, pokud Instance z nějakého důvodu selže
                inventoryManager = FindObjectOfType<InventoryManager>();
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
        if (Input.GetKeyDown(interactKey))
        {
            ZkontrolujInventar(); // Pojistka před interakcí
            TryPayAndOpen();
        }
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
        ZkontrolujInventar(); // Pojistka
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
        foreach (Transform child in requirementsParent)
        {
            child.gameObject.SetActive(false);
            uiPool.Add(child.gameObject);
        }
    }

    private void UpdateRequirementsUI()
    {
        if (requirementsPanel == null)
        {
            Debug.LogError("BossEntrance: Nemohu zobrazit panel, requirementsPanel je NULL!");
            // PŘIDÁNO: Zobrazení chyby, když selže panel
            if (debugErrorText != null) debugErrorText.SetActive(true);
            return;
        }

        // Pokud je náhodou celý hlavní Canvas vypnutý, zapneme ho
        if (WorldCanvas != null && !WorldCanvas.activeSelf) WorldCanvas.SetActive(true);

        // 🔥 PŘIDÁNO: Záplata na zapnutí vypnuté komponenty Canvas
        if (WorldCanvas != null)
        {
            Canvas canvasComp = WorldCanvas.GetComponent<Canvas>();
            if (canvasComp != null && !canvasComp.enabled) canvasComp.enabled = true;
        }

        requirementsPanel.SetActive(true);

        ZkontrolujInventar(); // Pojistka
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
                if (requirementsParent == null)
                {
                    Debug.LogError("BossEntrance: Chybí requirementsParent! Nemohu přidat položky.");
                    return;
                }
                row = Instantiate(requirementPrefab, requirementsParent);
                uiPool.Add(row);
            }

            int owned = inventoryManager.GetTotalItemCount(req.itemSO);

            // DŮLEŽITÉ: Názvy musí přesně odpovídat dětem v Prefabu!
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
            ZkontrolujInventar(); // Pojistka při vstupu hráče
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