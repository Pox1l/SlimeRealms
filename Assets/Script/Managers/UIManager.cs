using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("--- TUTORIAL UI ---")]
    public GameObject tutorialCanvas; // Tag: TutorialCanvas
    public GameObject tutorialRoot;   // NOVÉ: Tag: TutorialRoot (To se bude vypínat/zapínat)
    public bool isTutorialOpen { get; private set; } = false;

    [Header("--- GAME MENU (Inventář) ---")]
    public GameObject centralMenuCanvas;
    public GameObject centralMenuRoot;
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject skillTreePanel;
    public GameObject craftingPanel;

    [Header("--- CRYSTAL UI ---")]
    public GameObject crystalMenuCanvas;
    public GameObject crystalMenuRoot;

    [Header("--- PAUSE MENU ---")]
    public GameObject pauseMenuCanvas;
    public GameObject pauseMenuRoot;

    [Header("--- SETTINGS MENU ---")]
    public GameObject settingsMenuCanvas;
    public GameObject settingsMenuRoot;

    [Header("--- BOSS UI ---")]
    public GameObject bossMenuCanvas;
    public BossHealthUI bossScript;

    [Header("--- DEAD UI ---")]
    public GameObject deadMenuCanvas;
    public GameObject deadMenuRoot;
    private CanvasGroup deadCanvasGroup;

    [Header("--- SYSTEM ---")]
    public GameObject hudUI;

    // Reference
    private Transform player;
    private SettingsMenuController settingsScript;

    // Stavy
    public bool isGameMenuOpen { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isDead { get; private set; } = false;
    public bool isCrystalUIOpen { get; private set; } = false;
    public bool isBossFightActive { get; private set; } = false;

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
        tutorialCanvas = null; tutorialRoot = null; // Reset tutorialu
        isTutorialOpen = false;

        centralMenuCanvas = null; centralMenuRoot = null;
        pauseMenuCanvas = null; pauseMenuRoot = null;
        settingsMenuCanvas = null; settingsMenuRoot = null;
        deadMenuCanvas = null; deadMenuRoot = null; deadCanvasGroup = null;
        crystalMenuCanvas = null; crystalMenuRoot = null;
        bossMenuCanvas = null; bossScript = null;
        settingsScript = null;

        isGameMenuOpen = false;
        isPaused = false;
        isDead = false;
        isCrystalUIOpen = false;
        isBossFightActive = false;
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
        if (isDead) return;

        // 1. ESCAPE
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeInput();

        // 2. TAB
        if (!isPaused && !isCrystalUIOpen && !isTutorialOpen && Input.GetKeyDown(KeyCode.Tab)) ToggleGameMenu();

        // 3. Zkratky
        if (!isPaused && !isCrystalUIOpen && !isTutorialOpen) HandleShortcuts();
    }

    void HandleEscapeInput()
    {
        // Priorita 1: Settings (pokud jsme v nastavení, vracíme se do Pause menu)
        if (settingsMenuRoot != null && settingsMenuRoot.activeSelf)
        {
            OpenPauseMenu();
            return;
        }

        // Priorita 2: Krystal UI
        if (isCrystalUIOpen)
        {
            CloseCrystalMenu();
            return;
        }

        // Priorita 3: Pokud je hra zapauzovaná (i když je tutoriál na pozadí), chceme Resume
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        // Priorita 4: Game Menu (Inventář)
        if (isGameMenuOpen)
        {
            CloseGameMenu();
            return;
        }

        // Priorita 5: Tutoriál
        // Otevřeme Pause Menu, ale tutoriál zůstane ve stavu "otevřeno" na pozadí
        if (isTutorialOpen)
        {
            PauseGame();
            return;
        }

        // Priorita 6: Normální pauza
        PauseGame();
    }

    void FindEverythingInNewScene()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        GameObject hudObj = GameObject.FindGameObjectWithTag("HUD");
        if (hudObj != null) hudUI = hudObj;

        // --- TUTORIAL FINDER ---
        GameObject tutObj = GameObject.FindGameObjectWithTag("TutorialCanvas");
        if (tutObj != null)
        {
            tutorialCanvas = tutObj;

            // Hledáme Root a tlačítko uvnitř Canvasu
            foreach (Transform t in tutorialCanvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.CompareTag("TutorialRoot")) tutorialRoot = t.gameObject; // Hledá objekt s tagem TutorialRoot
                else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseTutorial);
            }
        }

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

        // 3. SETTINGS MENU
        GameObject sCanvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
        if (sCanvas != null)
        {
            settingsMenuCanvas = sCanvas;
            settingsScript = sCanvas.GetComponent<SettingsMenuController>();
            FindSettingsElements(settingsMenuCanvas.transform);
        }

        // 4. DEAD UI
        GameObject dCanvas = GameObject.FindGameObjectWithTag("DeadUICanvas");
        if (dCanvas != null)
        {
            deadMenuCanvas = dCanvas;
            FindDeadUIElements(deadMenuCanvas.transform);
        }

        // 5. CRYSTAL UI
        GameObject kCanvas = GameObject.FindGameObjectWithTag("KrystalCanvas");
        if (kCanvas != null)
        {
            crystalMenuCanvas = kCanvas;
            FindCrystalUIElements(crystalMenuCanvas.transform);
        }

        // 6. BOSS UI
        GameObject bCanvas = GameObject.FindGameObjectWithTag("BossUICanvas");
        if (bCanvas != null)
        {
            bossMenuCanvas = bCanvas;
            bossScript = bCanvas.GetComponentInChildren<BossHealthUI>(true);
            if (bossScript != null) bossScript.ToggleVisibility(false);
        }

        // Vypnout vše
        if (centralMenuRoot) centralMenuRoot.SetActive(false);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot) settingsMenuRoot.SetActive(false);
        if (deadMenuRoot) deadMenuRoot.SetActive(false);
        if (crystalMenuRoot) crystalMenuRoot.SetActive(false);
    }

    // --- TUTORIAL LOGIKA ---
    public void OpenTutorial()
    {
        // Zapínáme Root, pokud existuje, jinak celý Canvas (pojistka)
        if (tutorialRoot != null) tutorialRoot.SetActive(true);
        else if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
        else return;

        isTutorialOpen = true;
        Time.timeScale = 0f;

        // ZMĚNA: HUD zůstává viditelný
        if (hudUI != null) hudUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseTutorial()
    {
        isTutorialOpen = false;
        if (tutorialRoot != null) tutorialRoot.SetActive(false);
        else if (tutorialCanvas != null) tutorialCanvas.SetActive(false);

        Time.timeScale = 1f;
        if (hudUI != null) hudUI.SetActive(true);

        RefreshBossVisibility();
    }

    // --- OSTATNÍ HELPERY ---
    void FindSettingsElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("SettingsMenuRoot")) settingsMenuRoot = t.gameObject;
            else if (t.CompareTag("CloseSettingsBTN") || t.name == "XBTN") SetupButton(t, OpenPauseMenu);
        }
    }

    void FindPauseMenuElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("PauseMenuRoot")) pauseMenuRoot = t.gameObject;
            if (t.CompareTag("ResumeBTN") || t.name == "ReturnBTN") SetupButton(t, ResumeGame);
            else if (t.CompareTag("SettingsBTN") || t.name == "OptionBTN") SetupButton(t, OpenSettings);
            else if (t.CompareTag("QuitBTN") || t.name.Contains("Quit")) SetupButton(t, QuitGame);
            else if (t.CompareTag("ResetBTN") || t.name == "ResetPozBTN") SetupButton(t, ResetPlayerPosition);
        }
    }

    void FindCrystalUIElements(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("KrystalRoot")) crystalMenuRoot = t.gameObject;
            else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseCrystalMenu);
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

    private void RefreshBossVisibility()
    {
        if (bossScript == null) return;
        bool shouldShow = isBossFightActive && !isGameMenuOpen && !isPaused && !isCrystalUIOpen && !isDead && !isTutorialOpen;
        bossScript.ToggleVisibility(shouldShow);
    }

    public void StartBossFight(string name, int maxHP)
    {
        isBossFightActive = true;
        if (bossScript != null) bossScript.Init(name, maxHP);
        RefreshBossVisibility();
    }

    public void EndBossFight()
    {
        isBossFightActive = false;
        RefreshBossVisibility();
    }

    public void UpdateBossHP(int currentHP, int maxHP)
    {
        if (bossScript != null) bossScript.UpdateHealth(currentHP, maxHP);
    }

    public void PauseGame()
    {
        if (pauseMenuRoot == null) return;
        isPaused = true;
        Time.timeScale = 0;

        // HUD při pauze vypneme
        if (hudUI != null) hudUI.SetActive(false);

        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        isGameMenuOpen = false;

        // Pokud je tutoriál otevřený, dočasně ho skryjeme, aby bylo vidět Pause Menu
        if (isTutorialOpen)
        {
            if (tutorialRoot != null) tutorialRoot.SetActive(false);
            else if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        }

        pauseMenuRoot.SetActive(true);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
        RefreshBossVisibility();
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);

        // Návrat do tutoriálu (pokud byl aktivní)
        if (isTutorialOpen)
        {
            if (tutorialRoot != null) tutorialRoot.SetActive(true);
            else if (tutorialCanvas != null) tutorialCanvas.SetActive(true);

            Time.timeScale = 0f; // Tutoriál má hru zastavenou

            // ZMĚNA: HUD zapneme zpátky
            if (hudUI != null) hudUI.SetActive(true);
        }
        else
        {
            // Normální návrat do hry
            Time.timeScale = 1;
            if (hudUI != null) hudUI.SetActive(true);
        }

        RefreshBossVisibility();
    }

    public void OpenSettings()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot) settingsMenuRoot.SetActive(true);
        if (settingsScript) settingsScript.ResetTabs();
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        if (settingsMenuRoot) settingsMenuRoot.SetActive(false);
    }

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
        RefreshBossVisibility();
    }

    public void CloseCrystalMenu()
    {
        isCrystalUIOpen = false;
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);
        Time.timeScale = 1f;
        if (hudUI != null) hudUI.SetActive(true);
        RefreshBossVisibility();
    }

    public void ToggleGameMenu()
    {
        if (centralMenuRoot == null) return;
        isGameMenuOpen = !isGameMenuOpen;
        if (isGameMenuOpen)
        {
            centralMenuRoot.SetActive(true);
            Time.timeScale = 0;
            if (hudUI != null) hudUI.SetActive(false);
            OpenPanel(inventoryPanel);
        }
        else CloseGameMenu();
        RefreshBossVisibility();
    }

    public void CloseGameMenu()
    {
        isGameMenuOpen = false;
        Time.timeScale = 1;
        if (hudUI != null) hudUI.SetActive(true);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        RefreshBossVisibility();
    }

    public void OpenPanel(GameObject panel)
    {
        if (profilePanel) profilePanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (skillTreePanel) skillTreePanel.SetActive(false);
        if (craftingPanel) craftingPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
    }

    public void ShowDeathScreen()
    {
        if (deadMenuRoot == null) return;
        isDead = true;
        if (isGameMenuOpen) CloseGameMenu();
        if (isCrystalUIOpen) CloseCrystalMenu();
        if (isPaused) ResumeGame();
        if (isTutorialOpen) CloseTutorial();
        if (hudUI != null) hudUI.SetActive(false);
        deadMenuRoot.SetActive(true);
        Time.timeScale = 0f;
        RefreshBossVisibility();
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
            deadCanvasGroup.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }
        deadCanvasGroup.alpha = 1f;
        deadCanvasGroup.interactable = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RespawnPlayer()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame() => Application.Quit();
    public void ResetPlayerPosition() { }

    void SetupButton(Transform t, UnityEngine.Events.UnityAction action)
    {
        Button btn = t.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
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
}