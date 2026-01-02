using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Hlavní Objekty")]
    public GameObject pauseMenuRoot;    // Celý objekt "PauseMenu" (Pozadí + Tlačítka + Nadpis)
    public GameObject settingsMenuRoot; // Celý objekt "SettingsMenu"

    [Header("References")]
    private Transform player;
    private Transform currentRespawnPoint;
    private GameObject hudObject;

    public bool isPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        RefreshSceneLinks();

        // Na začátku zajistíme, že je všechno vypnuté
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. Pokud je otevřené Nastavení -> Zavřít nastavení a vrátit Pauzu
            if (settingsMenuRoot != null && settingsMenuRoot.activeSelf)
            {
                CloseSettings();
            }
            // 2. Pokud je otevřená Pauza -> Zavřít Pauzu a hrát
            else if (isPaused)
            {
                Resume();
            }
            // 3. Pokud se hraje -> Otevřít Pauzu
            else
            {
                Pause();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneLinks();
        Resume();
    }

    private void RefreshSceneLinks()
    {
        // ... tvoje existující hledání hráče ...
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj != null ? playerObj.transform : null;

        GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
        currentRespawnPoint = respawnObj != null ? respawnObj.transform : null;

        hudObject = GameObject.FindGameObjectWithTag("HUD");

        if (settingsMenuRoot == null)
        {
            GameObject settingsCanvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
            if (settingsCanvas != null)
            {
                Transform menuTrans = settingsCanvas.transform.Find("SettingsMenu");
                if (menuTrans != null)
                {
                    settingsMenuRoot = menuTrans.gameObject;
                }
            }
        }

        // --- NOVÉ: Automatické přiřazení zavíracího tlačítka ---
        if (settingsMenuRoot != null)
        {
            // Najde tlačítko podle jména v celém podstromu SettingsMenu
            // Použijeme GetComponentsInChildren, aby to našlo i neaktivní nebo hluboko schované objekty
            UnityEngine.UI.Button closeButton = null;
            foreach (var btn in settingsMenuRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                if (btn.name == "XBTN") // Musí se shodovat se jménem v Hierarchy
                {
                    closeButton = btn;
                    break;
                }
            }

            if (closeButton != null)
            {
                // Nejdřív odstraníme staré eventy, aby se to neduplikovalo při každém načtení scény
                closeButton.onClick.RemoveAllListeners();
                // Přiřadíme funkci CloseSettings
                closeButton.onClick.AddListener(CloseSettings);
            }
        }
    }

    // --- LOGIKA ---

    public void Resume()
    {
        // Vypneme obě okna
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);

        // Zapneme HUD
        if (hudObject != null) hudObject.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        RefreshSceneLinks();

        // Zapneme JEN Pauzu, nastavení musí být vypnuté
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);

        // Vypneme HUD
        if (hudObject != null) hudObject.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
    }

    // --- PŘEPÍNÁNÍ OKEN ---

    public void OpenSettings()
    {
        // Vypneme celé Pause Menu
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);

        // Zapneme celé Settings Menu
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(true);
    }

    public void CloseSettings()
    {
        // Vypneme Settings Menu
        if (settingsMenuRoot != null) settingsMenuRoot.SetActive(false);

        // Zapneme zpátky Pause Menu
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
    }

    // --- ZBYTEK ---
    public void ResetPlayerPosition()
    {
        if (player == null || currentRespawnPoint == null) return;
        player.position = currentRespawnPoint.position;
        Physics.SyncTransforms();
        Resume();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}