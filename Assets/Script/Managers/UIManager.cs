using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    /* ZATÍM ZAKOMENTOVÁNO - IRIS EFEKT
    [Header("--- TRANSITION EFFECTS (Novinka) ---")]
    // SEM přetáhni ten velký černý Image s materiálem IrisWipe
    public Image irisTransitionImage;
    public float transitionDuration = 1.5f;
    private int radiusID; // ID pro shader

    // 🔥 STATICKÁ proměnná, která přežije načtení scény.
    // Slouží jako "vzkaz" pro nově načtenou scénu, že má přehrát efekt.
    public static bool JustRespawned = false;
    */

    [Header("--- TUTORIAL UI ---")]
    public GameObject tutorialCanvas;
    public GameObject tutorialRoot;
    public bool isTutorialOpen { get; private set; } = false;

    [Header("--- TUTORIAL GAME UI ---")]
    public GameObject tutorialPanelGame;

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

    private Button resetPositionBtn;

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

            /* ZATÍM ZAKOMENTOVÁNO
            // Cache ID shaderu pro efektivitu
            radiusID = Shader.PropertyToID("_Radius");
            */
        }
        else if (Instance != this)
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

        /* ZATÍM ZAKOMENTOVÁNO
        // 🔥 KONTROLA PO NAČTENÍ SCÉNY:
        // Pokud jsme se právě respawnuli, spustíme efekt.
        if (JustRespawned)
        {
            PlayRespawnEffect();
            JustRespawned = false; // Resetujeme vzkaz, aby se nespouštěl při běžném loadování
        }
        else
        {
            // Pojistka: Pokud to není respawn, ujistíme se, že je černý image vypnutý.
            if (irisTransitionImage != null) irisTransitionImage.gameObject.SetActive(false);
        }
        */
    }

    /* ZATÍM ZAKOMENTOVÁNO - NOVÉ METODY PRO EFEKT

    public void PlayRespawnEffect()
    {
        if (irisTransitionImage != null)
        {
            // Ujistíme se, že neběží jiná coroutina (pro jistotu)
            StopCoroutine("IrisWipeRoutine");
            StartCoroutine("IrisWipeRoutine");
        }
        else
        {
            Debug.LogWarning("UIManager: Chybí reference na IrisTransitionImage!");
        }
    }

    IEnumerator IrisWipeRoutine()
    {
        // 1. Zapneme Image (černá obrazovka)
        irisTransitionImage.gameObject.SetActive(true);
        // Ujistíme se, že je navrchu
        irisTransitionImage.transform.SetAsLastSibling();

        Material mat = irisTransitionImage.material;

        // 2. Start: Úplná tma (Radius 0)
        mat.SetFloat(radiusID, 0f);

        // Malá pauza na černé obrazovce před roztmíváním
        yield return new WaitForSeconds(0.3f);

        float timer = 0f;

        // 3. Animace roztmívání
        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime; // Používáme unscaled, aby to jelo i při pauze
            float progress = timer / transitionDuration;

            // Plynulý přechod z 0 (tma) na 1 (vidět hra)
            float currentRadius = Mathf.SmoothStep(0f, 1.0f, progress);
            mat.SetFloat(radiusID, currentRadius);

            yield return null;
        }

        // 4. Konec: Ujistíme se, že je díra maximální a vypneme Image
        mat.SetFloat(radiusID, 1.5f);
        irisTransitionImage.gameObject.SetActive(false);
    }
    */


    // --- ZBYTEK PŮVODNÍHO KÓDU ---

    void ResetReferences()
    {
        tutorialCanvas = null; tutorialRoot = null;
        tutorialPanelGame = null;
        isTutorialOpen = false;

        centralMenuCanvas = null; centralMenuRoot = null;
        pauseMenuCanvas = null; pauseMenuRoot = null;
        settingsMenuCanvas = null; settingsMenuRoot = null;
        deadMenuCanvas = null; deadMenuRoot = null; deadCanvasGroup = null;
        crystalMenuCanvas = null; crystalMenuRoot = null;
        bossMenuCanvas = null; bossScript = null;
        settingsScript = null;

        resetPositionBtn = null;

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
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapeInput();
        if (!isPaused && !isCrystalUIOpen && !isTutorialOpen && Input.GetKeyDown(KeyCode.Tab)) ToggleGameMenu();
        if (!isPaused && !isCrystalUIOpen && !isTutorialOpen) HandleShortcuts();
    }

    void HandleEscapeInput()
    {
        if (settingsMenuRoot != null && settingsMenuRoot.activeSelf) { OpenPauseMenu(); return; }
        if (isCrystalUIOpen) { CloseCrystalMenu(); return; }
        if (isPaused) { ResumeGame(); return; }
        if (isGameMenuOpen) { CloseGameMenu(); return; }
        if (isTutorialOpen) { PauseGame(); return; }
        PauseGame();
    }

    void FindEverythingInNewScene()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        GameObject hudObj = GameObject.FindGameObjectWithTag("HUD");
        if (hudObj != null) hudUI = hudObj;

        /* ZATÍM ZAKOMENTOVÁNO - HLEDÁNÍ IRISU
        // 🔥 OPRAVA HLEDÁNÍ IRISU 🔥
        // Zkusíme ho najít. Protože jsi ho v Inspectoru ZAPNUL, teď ho najdeme.
        if (irisTransitionImage == null)
        {
            GameObject irisObj = GameObject.Find("IrisTransition");
            if (irisObj != null)
            {
                irisTransitionImage = irisObj.GetComponent<Image>();
                // Hned ho vypneme, ať nezaclání ve hře (pokud zrovna nerespawnujeme)
                if (!JustRespawned)
                {
                    irisTransitionImage.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("❌ UIManager: Nemůžu najít 'IrisTransition'! Ujisti se, že je v nové scéně ZAPNUTÝ a jmenuje se přesně takto.");
            }
        }
        */

        GameObject tutObj = GameObject.FindGameObjectWithTag("TutorialCanvas");
        if (tutObj != null)
        {
            tutorialCanvas = tutObj;
            foreach (Transform t in tutorialCanvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.CompareTag("TutorialRoot")) tutorialRoot = t.gameObject;
                else if (t.CompareTag("TutorialPanel"))
                {
                    tutorialPanelGame = t.gameObject;
                    if (tutorialPanelGame.activeSelf)
                    {
                        tutorialPanelGame.SetActive(false);
                    }
                }
                else if (t.CompareTag("CloseBTN")) SetupButton(t, CloseTutorial);
            }
        }

        GameObject cCanvas = GameObject.FindGameObjectWithTag("CentralMenuCanvas");
        if (cCanvas != null) { centralMenuCanvas = cCanvas; FindGameMenuElements(centralMenuCanvas.transform); }
        GameObject pCanvas = GameObject.FindGameObjectWithTag("PauseMenuCanvas");
        if (pCanvas != null) { pauseMenuCanvas = pCanvas; FindPauseMenuElements(pauseMenuCanvas.transform); }
        GameObject sCanvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
        if (sCanvas != null) { settingsMenuCanvas = sCanvas; settingsScript = sCanvas.GetComponent<SettingsMenuController>(); FindSettingsElements(settingsMenuCanvas.transform); }
        GameObject dCanvas = GameObject.FindGameObjectWithTag("DeadUICanvas");
        if (dCanvas != null) { deadMenuCanvas = dCanvas; FindDeadUIElements(deadMenuCanvas.transform); }
        GameObject kCanvas = GameObject.FindGameObjectWithTag("KrystalCanvas");
        if (kCanvas != null) { crystalMenuCanvas = kCanvas; FindCrystalUIElements(crystalMenuCanvas.transform); }
        GameObject bCanvas = GameObject.FindGameObjectWithTag("BossUICanvas");
        if (bCanvas != null) { bossMenuCanvas = bCanvas; bossScript = bCanvas.GetComponentInChildren<BossHealthUI>(true); if (bossScript != null) bossScript.ToggleVisibility(false); }

        if (centralMenuRoot) centralMenuRoot.SetActive(false);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot) settingsMenuRoot.SetActive(false);
        if (deadMenuRoot) deadMenuRoot.SetActive(false);
        if (crystalMenuRoot) crystalMenuRoot.SetActive(false);
    }

    public void OpenTutorial()
    {
        if (tutorialRoot != null) tutorialRoot.SetActive(true);
        else if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
        else return;
        isTutorialOpen = true;

        if (resetPositionBtn != null) resetPositionBtn.interactable = false;

        Time.timeScale = 0f;
        if (hudUI != null) hudUI.SetActive(true);
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
    }

    public void CloseTutorial()
    {
        isTutorialOpen = false;

        if (resetPositionBtn != null) resetPositionBtn.interactable = true;

        if (tutorialRoot != null) tutorialRoot.SetActive(false);
        else if (tutorialCanvas != null) tutorialCanvas.SetActive(false);

        // Zobrazení panelu jen pokud tutoriál reálně probíhá a neskončil
        if (tutorialPanelGame != null && TutorialManager.Instance != null && !TutorialManager.Instance.IsCompleted)
        {
            tutorialPanelGame.SetActive(true);
        }

        Time.timeScale = 1f;
        if (hudUI != null) hudUI.SetActive(true);
        RefreshBossVisibility();
    }

    public void HideTutorialPanelGame()
    {
        if (tutorialPanelGame != null) tutorialPanelGame.SetActive(false);
    }

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
            else if (t.CompareTag("ResetBTN") || t.name == "ResetPozBTN")
            {
                resetPositionBtn = t.GetComponent<Button>();
                SetupButton(t, ResetPlayerPosition);
            }
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

    public void StartBossFight(string name, int maxHP) { isBossFightActive = true; if (bossScript != null) bossScript.Init(name, maxHP); RefreshBossVisibility(); }
    public void EndBossFight() { isBossFightActive = false; RefreshBossVisibility(); }
    public void UpdateBossHP(int currentHP, int maxHP) { if (bossScript != null) bossScript.UpdateHealth(currentHP, maxHP); }

    public void PauseGame()
    {
        if (pauseMenuRoot == null) return;
        isPaused = true; Time.timeScale = 0;
        if (hudUI != null) hudUI.SetActive(false);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);

        // Skrýt tutorial panel při pauze
        if (tutorialPanelGame != null) tutorialPanelGame.SetActive(false);

        isGameMenuOpen = false;
        if (isTutorialOpen) { if (tutorialRoot != null) tutorialRoot.SetActive(false); else if (tutorialCanvas != null) tutorialCanvas.SetActive(false); }
        pauseMenuRoot.SetActive(true); if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false); RefreshBossVisibility();
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
        if (centralMenuRoot != null) centralMenuRoot.SetActive(false);
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);

        // Zobrazit tutorial panel zpět, pokud má běžet
        if (tutorialPanelGame != null && TutorialManager.Instance != null && !TutorialManager.Instance.IsCompleted && !isTutorialOpen)
        {
            tutorialPanelGame.SetActive(true);
        }

        if (isTutorialOpen) { if (tutorialRoot != null) tutorialRoot.SetActive(true); else if (tutorialCanvas != null) tutorialCanvas.SetActive(true); Time.timeScale = 0f; if (hudUI != null) hudUI.SetActive(true); }
        else { Time.timeScale = 1; if (hudUI != null) hudUI.SetActive(true); }
        RefreshBossVisibility();
    }

    public void OpenSettings() { if (pauseMenuRoot) pauseMenuRoot.SetActive(false); if (settingsMenuRoot) settingsMenuRoot.SetActive(true); if (settingsScript) settingsScript.ResetTabs(); }
    public void OpenPauseMenu() { if (pauseMenuRoot) pauseMenuRoot.SetActive(true); if (settingsMenuRoot) settingsMenuRoot.SetActive(false); }
    public void ToggleCrystalUI() { if (isCrystalUIOpen) CloseCrystalMenu(); else OpenCrystalMenu(); }
    public void OpenCrystalMenu()
    {
        if (crystalMenuRoot == null) return;
        isCrystalUIOpen = true;
        crystalMenuRoot.SetActive(true);
        Time.timeScale = 0f;
        if (hudUI != null) hudUI.SetActive(false);

        // Skrýt tutorial panel
        if (tutorialPanelGame != null) tutorialPanelGame.SetActive(false);

        if (isGameMenuOpen) CloseGameMenu();
        RefreshBossVisibility();
    }
    public void CloseCrystalMenu()
    {
        isCrystalUIOpen = false;
        if (crystalMenuRoot != null) crystalMenuRoot.SetActive(false);
        Time.timeScale = 1f;
        if (hudUI != null) hudUI.SetActive(true);

        // Zobrazit tutorial panel
        if (tutorialPanelGame != null && TutorialManager.Instance != null && !TutorialManager.Instance.IsCompleted)
        {
            tutorialPanelGame.SetActive(true);
        }

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

            // Skrýt tutorial panel
            if (tutorialPanelGame != null) tutorialPanelGame.SetActive(false);

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

        // Zobrazit tutorial panel
        if (tutorialPanelGame != null && TutorialManager.Instance != null && !TutorialManager.Instance.IsCompleted)
        {
            tutorialPanelGame.SetActive(true);
        }

        RefreshBossVisibility();
    }
    public void OpenPanel(GameObject panel) { if (profilePanel) profilePanel.SetActive(false); if (inventoryPanel) inventoryPanel.SetActive(false); if (skillTreePanel) skillTreePanel.SetActive(false); if (craftingPanel) craftingPanel.SetActive(false); if (panel != null) panel.SetActive(true); }

    public void ShowDeathScreen()
    {
        if (deadMenuRoot == null) return;
        isDead = true;
        if (isGameMenuOpen) CloseGameMenu(); if (isCrystalUIOpen) CloseCrystalMenu(); if (isPaused) ResumeGame(); if (isTutorialOpen) CloseTutorial();
        if (hudUI != null) hudUI.SetActive(false);

        // Skrýt tutorial panel
        if (tutorialPanelGame != null) tutorialPanelGame.SetActive(false);

        deadMenuRoot.SetActive(true);
        Time.timeScale = 0f;
        RefreshBossVisibility();
        if (deadCanvasGroup != null) { deadCanvasGroup.alpha = 0f; deadCanvasGroup.interactable = false; deadCanvasGroup.blocksRaycasts = true; StartCoroutine(FadeInDeadUI(1.5f)); }
        else { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
    }

    IEnumerator FadeInDeadUI(float duration) { float timer = 0f; while (timer < duration) { timer += Time.unscaledDeltaTime; deadCanvasGroup.alpha = Mathf.Clamp01(timer / duration); yield return null; } deadCanvasGroup.alpha = 1f; deadCanvasGroup.interactable = true; Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

    public void RespawnPlayer()
    {
        /* ZATÍM ZAKOMENTOVÁNO
        // Nastavíme vzkaz, že příští načtení scény je respawn
        JustRespawned = true;
        */

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame() => Application.Quit();
    public void ResetPlayerPosition()
    {
        if (player != null)
        {
            // Tady doplň svou logiku pro pozici, kam se má hráč portnout
            // Např: player.position = new Vector3(0, 5, 0);

            Debug.Log("Hráč resetován na startovní pozici.");
        }

        // KLÍČOVÁ ČÁST: Musíš zavolat ResumeGame, aby se vypnula pauza, 
        // pustil se čas, skrylo se UI a zamkl se kurzor.
        ResumeGame();
    }
    void SetupButton(Transform t, UnityEngine.Events.UnityAction action) { Button btn = t.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(action); } }
    void HandleShortcuts() { if (isGameMenuOpen) { if (Input.GetKeyDown(KeyCode.P)) OpenPanel(profilePanel); if (Input.GetKeyDown(KeyCode.I)) OpenPanel(inventoryPanel); if (Input.GetKeyDown(KeyCode.L)) OpenPanel(skillTreePanel); if (Input.GetKeyDown(KeyCode.C)) OpenPanel(craftingPanel); } else { if (Input.GetKeyDown(KeyCode.P)) { ToggleGameMenu(); OpenPanel(profilePanel); } if (Input.GetKeyDown(KeyCode.I)) { ToggleGameMenu(); OpenPanel(inventoryPanel); } if (Input.GetKeyDown(KeyCode.L)) { ToggleGameMenu(); OpenPanel(skillTreePanel); } if (Input.GetKeyDown(KeyCode.C)) { ToggleGameMenu(); OpenPanel(craftingPanel); } } }
}