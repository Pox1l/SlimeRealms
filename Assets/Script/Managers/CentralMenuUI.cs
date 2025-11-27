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

    [Header("HUD (Hledá se globálně podle tagu 'HUD')")]
    public GameObject hudUI;

    private bool isMenuOpen = false;

    void Awake()
    {
        // 1. Singleton + DontDestroyOnLoad pro Managera
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

    // Tato metoda se zavolá vždy, když se načte nová scéna
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset proměnných
        menuCanvas = null;
        centralMenuRoot = null;
        isMenuOpen = false;

        // Spustíme kaskádové hledání
        FindEverythingInNewScene();
    }

    void Update()
    {
        // Pokud se nám nepodařilo najít UI (např. jsme v Main Menu scéně), nic neděláme
        if (centralMenuRoot == null) return;

        if (Input.GetKeyDown(KeyCode.Tab)) ToggleMenu();

        if (isMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CloseMenu();
            if (Input.GetKeyDown(KeyCode.P)) OpenProfile();
            if (Input.GetKeyDown(KeyCode.I)) OpenInventory();
            if (Input.GetKeyDown(KeyCode.L)) OpenSkillTree();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P)) { ToggleMenu(); OpenProfile(); }
            if (Input.GetKeyDown(KeyCode.I)) { ToggleMenu(); OpenInventory(); }
            if (Input.GetKeyDown(KeyCode.L)) { ToggleMenu(); OpenSkillTree(); }
        }
    }

    // 🔍 KASKÁDOVÉ HLEDÁNÍ
    void FindEverythingInNewScene()
    {
        // 1. Najdi HUD (ten je obvykle mimo toto menu)
        GameObject hudObj = GameObject.FindGameObjectWithTag("HUD");
        if (hudObj != null) hudUI = hudObj;

        // 2. Najdi HLAVNÍ CANVAS pro toto menu
        GameObject canvasObj = GameObject.FindGameObjectWithTag("CentralMenuCanvas");
        if (canvasObj != null)
        {
            menuCanvas = canvasObj;

            // 3. Najdi ROOT uvnitř Canvasu (hledáme i v neaktivních objektech - true)
            // Musíme projít děti Canvasu, abychom našli ten správný Root
            Transform[] children = menuCanvas.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.CompareTag("CentralMenuRoot"))
                {
                    centralMenuRoot = child.gameObject;
                    break; // Našli jsme root, končíme cyklus
                }
            }

            // 4. Pokud jsme našli Root, prohledáme jeho vnitřek (Panely a Tlačítka)
            if (centralMenuRoot != null)
            {
                FindPanelsAndButtons(centralMenuRoot);

                // Zajistíme, že menu je na začátku zavřené
                centralMenuRoot.SetActive(false);
            }
            else
            {
                Debug.LogWarning("❌ Canvas nalezen, ale chybí v něm objekt s tagem 'CentralMenuRoot'!");
            }
        }
        else
        {
            // Toto je normální v Main Menu scéně, kde třeba herní UI není
            // Debug.Log("⚠️ V této scéně není 'CentralMenuCanvas'.");
        }
    }

    void FindPanelsAndButtons(GameObject root)
    {
        // Prohledáme všechny děti Rootu (i neaktivní)
        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allChildren)
        {
            // Hledání panelů
            if (t.CompareTag("ProfilePanel")) profilePanel = t.gameObject;
            else if (t.CompareTag("InventoryPanel")) inventoryPanel = t.gameObject;
            else if (t.CompareTag("SkillTreePanel")) skillTreePanel = t.gameObject;

            // Hledání a nastavení tlačítek
            else if (t.CompareTag("ProfileBTN")) SetupButton(t, OpenProfile);
            else if (t.CompareTag("InventoryBTN")) SetupButton(t, OpenInventory);
            else if (t.CompareTag("SkillTreeBTN")) SetupButton(t, OpenSkillTree);
            else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseMenu);
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
        // Pojistka, kdyby se root nenašel
        if (centralMenuRoot == null) return;

        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            centralMenuRoot.SetActive(true);
            Time.timeScale = 0;
            if (hudUI != null) hudUI.SetActive(false);
            OpenInventory(); // Defaultně otevřeme inventář
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
    }
}