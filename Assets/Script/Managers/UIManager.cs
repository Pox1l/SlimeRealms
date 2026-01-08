using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("--- GAME MENU (Inventář) ---")]
    public GameObject centralMenuCanvas;
    public GameObject centralMenuRoot;
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject skillTreePanel;
    public GameObject craftingPanel;

    [Header("--- CRYSTAL UI (Obchod/Upgrade) ---")]
    public GameObject crystalMenuCanvas; // Tag: KrystalCanvas
    public GameObject crystalMenuRoot;   // Tag: KrystalRoot (vnitřní panel)

    [Header("--- PAUSE MENU ---")]
    public GameObject pauseMenuCanvas;
    public GameObject pauseMenuRoot;
    public GameObject settingsMenuRoot;

    [Header("--- DEAD UI (Smrt) ---")]
    public GameObject deadMenuCanvas;   // Tag: DeadUICanvas
    public GameObject deadMenuRoot;     // Tag: DeadUIRoot
    private CanvasGroup deadCanvasGroup; // Pro Fade efekt

    [Header("--- SYSTEM ---")]
    public GameObject hudUI;

    // Reference
    private Transform player;
    private Transform currentRespawnPoint;

    // Stavy
    public bool isGameMenuOpen { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isDead { get; private set; } = false;
    public bool isCrystalUIOpen { get; private set; } = false;

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
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromPlayerEvents();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetReferences();
        FindEverythingInNewScene();
        SubscribeToPlayerEvents();
    }

    void ResetReferences()
    {
        centralMenuCanvas = null; centralMenuRoot = null;
        pauseMenuCanvas = null; pauseMenuRoot = null; settingsMenuRoot = null;
        deadMenuCanvas = null; deadMenuRoot = null; deadCanvasGroup = null;
        crystalMenuCanvas = null; crystalMenuRoot = null;

        isGameMenuOpen = false;
        isPaused = false;
        isDead = false;
        isCrystalUIOpen = false; // Reset stavu krystalu
        Time.timeScale = 1;
    }

    void SubscribeToPlayerEvents()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied += ShowDeathScreen;
        }
    }

    void UnsubscribeFromPlayerEvents()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowDeathScreen;
        }
    }

    void Update()
    {
        // 🚨 KDYŽ JSI MRTVÝ, NIC JINÉHO NEFUNGUJE 🚨
        if (isDead) return;

        // 1. ESCAPE
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeInput();

        // 2. TAB
        if (!isPaused && !isCrystalUIOpen && Input.GetKeyDown(KeyCode.Tab)) ToggleGameMenu();

        // 3. Zkratky
        if (!isPaused && !isCrystalUIOpen) HandleShortcuts();
    }

    // --- DEAD UI LOGIKA ---
    public void ShowDeathScreen()
    {
        if (deadMenuRoot == null) return;

        Debug.Log("💀 Hráč zemřel -> Zobrazuji Dead UI");
        isDead = true;

        if (isGameMenuOpen) CloseGameMenu();
        if (isCrystalUIOpen) CloseCrystalMenu();
        if (isPaused) ResumeGame();
        if (hudUI != null) hudUI.SetActive(false);

        deadMenuRoot.SetActive(true);
        Time.timeScale = 0f;

        if (deadCanvasGroup != null)
        {
            deadCanvasGroup.alpha = 0f;
            deadCanvasGroup.interactable = false;
            deadCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeInDeadUI(1.5f));
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    IEnumerator FadeInDeadUI(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / duration);
            deadCanvasGroup.alpha = alpha;
            yield return null;
        }

        deadCanvasGroup.alpha = 1f;
        deadCanvasGroup.interactable = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RespawnPlayer()
    {
        Debug.Log("🔄 Respawn...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- UPDATE AUTOMATICKÉHO HLEDÁNÍ ---

    void FindEverythingInNewScene()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        GameObject hudObj = GameObject.FindGameObjectWithTag("HUD");
        if (hudObj != null) hudUI = hudObj;

        // 1. GAME MENU
        GameObject cCanvas = GameObject.FindGameObjectWithTag("CentralMenuCanvas");
        if (cCanvas != null)
        {
            centralMenuCanvas = cCanvas;
            FindGameMenuElements(centralMenuCanvas.transform);
        }

        // 2. PAUSE MENU
        GameObject pCanvas = GameObject.FindGameObjectWithTag("PauseMenuCanvas");
        if (pCanvas != null)
        {
            pauseMenuCanvas = pCanvas;
            FindPauseMenuElements(pauseMenuCanvas.transform);
        }

        // 3. DEAD UI
        GameObject dCanvas = GameObject.FindGameObjectWithTag("DeadUICanvas");
        if (dCanvas != null)
        {
            deadMenuCanvas = dCanvas;
            FindDeadUIElements(deadMenuCanvas.transform);
        }

        // --- PŘIDÁNO: 4. CRYSTAL UI ---
        GameObject kCanvas = GameObject.FindGameObjectWithTag("KrystalCanvas");
        if (kCanvas != null)
        {
            crystalMenuCanvas = kCanvas;
            FindCrystalUIElements(crystalMenuCanvas.transform);
        }

        // Vypnout vše na startu
        if (centralMenuRoot) centralMenuRoot.SetActive(false);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot) settingsMenuRoot.SetActive(false);
        if (deadMenuRoot) deadMenuRoot.SetActive(false);

        // --- PŘIDÁNO: Vypnutí Krystalu na startu ---
        if (crystalMenuRoot) crystalMenuRoot.SetActive(false);
    }

    // --- PŘIDÁNO: HLEDÁNÍ KRYSTAL PRVKŮ ---
    void FindCrystalUIElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("KrystalRoot"))
            {
                crystalMenuRoot = t.gameObject;
            }
            // Pokud bys chtěl mít uvnitř krystalu tlačítko na zavření s tagem CloseBTN
            else if (t.CompareTag("CloseBTN"))
            {
                SetupButton(t, CloseCrystalMenu);
            }
        }
    }

    void FindDeadUIElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("DeadUIRoot"))
            {
                deadMenuRoot = t.gameObject;
                deadCanvasGroup = deadMenuRoot.GetComponent<CanvasGroup>();
                if (deadCanvasGroup == null) deadCanvasGroup = deadMenuRoot.AddComponent<CanvasGroup>();
            }
            else if (t.CompareTag("RespawnBTN")) SetupButton(t, RespawnPlayer);
            else if (t.CompareTag("QuitBTN")) SetupButton(t, QuitGame);
        }
    }

    void FindGameMenuElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("CentralMenuRoot")) centralMenuRoot = t.gameObject;
            else if (t.CompareTag("ProfilePanel")) profilePanel = t.gameObject;
            else if (t.CompareTag("InventoryPanel")) inventoryPanel = t.gameObject;
            else if (t.CompareTag("SkillTreePanel")) skillTreePanel = t.gameObject;
            else if (t.CompareTag("CraftingPanel")) craftingPanel = t.gameObject;

            else if (t.CompareTag("ProfileBTN")) SetupButton(t, () => OpenPanel(profilePanel));
            else if (t.CompareTag("InventoryBTN")) SetupButton(t, () => OpenPanel(inventoryPanel));
            else if (t.CompareTag("SkillTreeBTN")) SetupButton(t, () => OpenPanel(skillTreePanel));
            else if (t.CompareTag("CraftingBTN")) SetupButton(t, () => OpenPanel(craftingPanel));
            else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseGameMenu);

            else if (t.CompareTag("ContextMenuUI"))
            {
                var sceneUI = t.GetComponent<ContextMenuSceneUI>();
                if (sceneUI != null && InventoryContextMenu.Instance != null) InventoryContextMenu.Instance.RegisterSceneUI(sceneUI);
            }
        }
    }

    void FindPauseMenuElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("PauseMenuRoot")) pauseMenuRoot = t.gameObject;
            else if (t.CompareTag("SettingsMenuRoot")) settingsMenuRoot = t.gameObject;

            else if (t.CompareTag("ResumeBTN")) SetupButton(t, ResumeGame);
            else if (t.CompareTag("SettingsBTN")) SetupButton(t, OpenSettings);
            else if (t.CompareTag("QuitBTN")) SetupButton(t, QuitGame);
            else if (t.CompareTag("ResetBTN")) SetupButton(t, ResetPlayerPosition);
            else if (t.CompareTag("CloseSettingsBTN")) SetupButton(t, OpenPauseMenu);
        }
    }

    void HandleEscapeInput()
    {
        if (settingsMenuRoot != null && settingsMenuRoot.activeSelf) { OpenPauseMenu(); return; }
        if (isCrystalUIOpen) { CloseCrystalMenu(); return; }
        if (isPaused) { ResumeGame(); return; }
        if (isGameMenuOpen) { CloseGameMenu(); return; }
        PauseGame();
    }

    void HandleShortcuts()
    {
        if (isGameMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.P)) OpenPanel(profilePanel);
            if (Input.GetKeyDown(KeyCode.I)) OpenPanel(inventoryPanel);
            if (Input.GetKeyDown(KeyCode.L)) OpenPanel(skillTreePanel);
            if (Input.GetKeyDown(KeyCode.C)) OpenPanel(craftingPanel);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P)) { ToggleGameMenu(); OpenPanel(profilePanel); }
            if (Input.GetKeyDown(KeyCode.I)) { ToggleGameMenu(); OpenPanel(inventoryPanel); }
            if (Input.GetKeyDown(KeyCode.L)) { ToggleGameMenu(); OpenPanel(skillTreePanel); }
            if (Input.GetKeyDown(KeyCode.C)) { ToggleGameMenu(); OpenPanel(craftingPanel); }
        }
    }

    // --- CRYSTAL UI LOGIKA ---

    public void ToggleCrystalUI()
    {
        if (isCrystalUIOpen) CloseCrystalMenu();
        else OpenCrystalMenu();
    }

    public void OpenCrystalMenu()
    {
        if (crystalMenuRoot == null) return;

        isCrystalUIOpen = true;
        crystalMenuRoot.SetActive(true);
        Time.timeScale = 0f;
        if (hudUI != null) hudUI.SetActive(false);
        if (isGameMenuOpen) CloseGameMenu();
    }

    public void CloseCrystalMenu()
    {
        isCrystalUIOpen = false;
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);
        Time.timeScale = 1f;
        if (hudUI != null) hudUI.SetActive(true);
    }

    public void PauseGame()
    {
        if (pauseMenuRoot == null) return;
        isPaused = true; Time.timeScale = 0;
        if (hudUI != null) hudUI.SetActive(false);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        isGameMenuOpen = false;
        pauseMenuRoot.SetActive(true);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
    }
    public void ResumeGame()
    {
        isPaused = false; Time.timeScale = 1;
        if (hudUI != null) hudUI.SetActive(true);
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);
    }
    public void OpenPauseMenu()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
    }
    public void OpenSettings()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(true);
    }
    public void ToggleGameMenu()
    {
        if (centralMenuRoot == null) return;
        isGameMenuOpen = !isGameMenuOpen;
        if (isGameMenuOpen)
        {
            centralMenuRoot.SetActive(true); Time.timeScale = 0;
            if (hudUI != null) hudUI.SetActive(false);
            OpenPanel(inventoryPanel);
        }
        else CloseGameMenu();
    }
    public void CloseGameMenu()
    {
        isGameMenuOpen = false; Time.timeScale = 1;
        if (hudUI != null) hudUI.SetActive(true);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
    }
    public void OpenPanel(GameObject panel)
    {
        if (profilePanel) profilePanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (skillTreePanel) skillTreePanel.SetActive(false);
        if (craftingPanel) craftingPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
    }
    public void QuitGame() => Application.Quit();
    public void ResetPlayerPosition() { /* Tvoje logika */ }

    void SetupButton(Transform t, UnityEngine.Events.UnityAction action)
    {
        Button btn = t.GetComponent<Button>();
        if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(action); }
    }
}