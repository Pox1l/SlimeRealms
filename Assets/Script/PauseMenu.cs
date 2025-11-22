using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // 💡 1. Singleton: Veřejný přístup, abychom věděli, že existuje
    public static PauseMenu Instance;

    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Player")]
    private Transform player;

    [Header("Respawn point (jen v aktuální scéně)")]
    private Transform currentRespawnPoint;

    private bool isPaused = false;

    void Awake()
    {
        // 💡 2. Kontrola duplicit
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🛠️ ZDE: Odpojíme objekt, aby byl v kořenu (root)
        transform.SetParent(null);

        // 💡 3. Hlavní příkaz
        DontDestroyOnLoad(gameObject);

        // Registrace eventu
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ⚠️ DŮLEŽITÉ: Když objekt nakonec zanikne (např. vypnutí hry), musíme event zrušit
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        RefreshSceneLinks(); // Prvotní nalezení hráče
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // Tato metoda se zavolá automaticky po každém načtení scény
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneLinks();
    }

    /// <summary>
    /// Najde znovu Playera a respawn point v AKTUÁLNÍ scéně.
    /// </summary>
    private void RefreshSceneLinks()
    {
        // Hledání hráče
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            // Debug.LogWarning("PauseMenu: Player nenalezen v nové scéně.");
            player = null;

        // Hledání respawnu
        currentRespawnPoint = null;
        GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnObj != null)
        {
            currentRespawnPoint = respawnObj.transform;
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResetPlayerPosition()
    {
        if (player == null)
        {
            Debug.LogWarning("ResetPlayerPosition: Player nebyl nalezen.");
            return;
        }

        if (currentRespawnPoint == null)
        {
            Debug.LogWarning("ResetPlayerPosition: Chybí Respawn point.");
            return;
        }

        // Teleport
        player.position = currentRespawnPoint.position;

        // Zavřeme menu a obnovíme čas po respawnu (volitelné)
        Resume();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}