using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CentralMenuUI : MonoBehaviour
{
    public static CentralMenuUI Instance;

    [Header("1. Krok: Canvas (Hledá se podle tagu 'CentralMenuCanvas')")]
    public GameObject menuCanvas;

    [Header("2. Krok: Root Menu (Hledá se uvnitř Canvasu podle tagu 'CentralMenuRoot')")]
    public GameObject centralMenuRoot;

    [Header("3. Krok: Panely (Hledají se uvnitř Rootu)")]
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject skillTreePanel;
    public GameObject craftingPanel; // PŘIDÁNO: Crafting Panel

    [Header("HUD (Hledá se globálně podle tagu 'HUD')")]
    public GameObject hudUI;

    private bool isMenuOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset proměnných
        menuCanvas = null;
        centralMenuRoot = null;
        isMenuOpen = false;

        // Spustíme hledání
        FindEverythingInNewScene();
    }

    void Update()
    {
        if (centralMenuRoot == null) return;

        if (Input.GetKeyDown(KeyCode.Tab)) ToggleMenu();

        if (isMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CloseMenu();
            if (Input.GetKeyDown(KeyCode.P)) OpenProfile();
            if (Input.GetKeyDown(KeyCode.I)) OpenInventory();
            if (Input.GetKeyDown(KeyCode.L)) OpenSkillTree();
            if (Input.GetKeyDown(KeyCode.C)) OpenCrafting(); // PŘIDÁNO: Otevření craftingu v menu
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P)) { ToggleMenu(); OpenProfile(); }
            if (Input.GetKeyDown(KeyCode.I)) { ToggleMenu(); OpenInventory(); }
            if (Input.GetKeyDown(KeyCode.L)) { ToggleMenu(); OpenSkillTree(); }
            if (Input.GetKeyDown(KeyCode.C)) { ToggleMenu(); OpenCrafting(); } // PŘIDÁNO: Otevření menu rovnou na craftingu
        }
    }

    // 🔍 KASKÁDOVÉ HLEDÁNÍ
    void FindEverythingInNewScene()
    {
        GameObject hudObj = GameObject.FindGameObjectWithTag("HUD");
        if (hudObj != null) hudUI = hudObj;

        GameObject canvasObj = GameObject.FindGameObjectWithTag("CentralMenuCanvas");
        if (canvasObj != null)
        {
            menuCanvas = canvasObj;
            Transform[] children = menuCanvas.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.CompareTag("CentralMenuRoot"))
                {
                    centralMenuRoot = child.gameObject;
                    break;
                }
            }

            if (centralMenuRoot != null)
            {
                FindPanelsAndButtons(centralMenuRoot);
                centralMenuRoot.SetActive(false);
            }
            else
            {
                Debug.LogWarning("❌ Canvas nalezen, ale chybí v něm objekt s tagem 'CentralMenuRoot'!");
            }
        }
    }

    void FindPanelsAndButtons(GameObject root)
    {
        // Prohledáme všechny děti Rootu (i neaktivní - true)
        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allChildren)
        {
            // --- HLEDÁNÍ PANELŮ ---
            if (t.CompareTag("ProfilePanel")) profilePanel = t.gameObject;
            else if (t.CompareTag("InventoryPanel")) inventoryPanel = t.gameObject;
            else if (t.CompareTag("SkillTreePanel")) skillTreePanel = t.gameObject;
            else if (t.CompareTag("CraftingPanel")) craftingPanel = t.gameObject; // PŘIDÁNO: Hledání panelu

            // --- HLEDÁNÍ TLAČÍTEK ---
            else if (t.CompareTag("ProfileBTN")) SetupButton(t, OpenProfile);
            else if (t.CompareTag("InventoryBTN")) SetupButton(t, OpenInventory);
            else if (t.CompareTag("SkillTreeBTN")) SetupButton(t, OpenSkillTree);
            else if (t.CompareTag("CraftingBTN")) SetupButton(t, OpenCrafting); // PŘIDÁNO: Hledání tlačítka
            else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseMenu);

            // 🔥 NOVÉ: AUTOMATICKÉ PROPOJENÍ KONTEXTOVÉHO MENU 🔥
            else if (t.CompareTag("ContextMenuUI"))
            {
                // Najdeme ten pomocný skript, co drží odkazy na tlačítka
                ContextMenuSceneUI sceneUI = t.GetComponent<ContextMenuSceneUI>();

                if (sceneUI != null)
                {
                    // Najdeme hlavního Managera a předáme mu to
                    if (InventoryContextMenu.Instance != null)
                    {
                        InventoryContextMenu.Instance.RegisterSceneUI(sceneUI);
                        // Debug.Log("✅ CentralMenuUI: Kontextové menu nalezeno a propojeno!");
                    }
                }
            }
        }
    }

    void SetupButton(Transform t, UnityEngine.Events.UnityAction action)
    {
        Button btn = t.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    // ---- Logika UI ----

    public void ToggleMenu()
    {
        if (centralMenuRoot == null) return;

        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            centralMenuRoot.SetActive(true);
            Time.timeScale = 0;
            if (hudUI != null) hudUI.SetActive(false);
            OpenInventory();
        }
        else
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        Time.timeScale = 1;
        if (hudUI != null) hudUI.SetActive(true);
        CloseAllPanels();
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
    }

    public void OpenProfile() => ShowPanel(profilePanel);
    public void OpenInventory() => ShowPanel(inventoryPanel);
    public void OpenSkillTree() => ShowPanel(skillTreePanel);
    public void OpenCrafting() => ShowPanel(craftingPanel); // PŘIDÁNO

    void ShowPanel(GameObject panel)
    {
        CloseAllPanels();
        if (panel != null) panel.SetActive(true);
    }

    void CloseAllPanels()
    {
        if (profilePanel) profilePanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (skillTreePanel) skillTreePanel.SetActive(false);
        if (craftingPanel) craftingPanel.SetActive(false); // PŘIDÁNO
    }
}