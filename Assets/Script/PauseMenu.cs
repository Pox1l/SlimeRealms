using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Player")]
    private Transform player;

    [Header("Respawn point (jen v aktuální scéně)")]
    private Transform currentRespawnPoint;

    private bool isPaused = false;

    // Singleton + DontDestroyOnLoad, aby menu přežilo mezi scénami
    private static PauseMenu instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // reaguj na načtení nové scény
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        RefreshSceneLinks(); // najdi playera + respawn pro první scénu
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // ⬇ zavolá se po každém načtení nové scény
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneLinks();
    }

    /// <summary>
    /// Najde znovu Playera a respawn point v AKTUÁLNÍ scéně.
    /// </summary>
    private void RefreshSceneLinks()
    {
        // Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            player = null;

        
        currentRespawnPoint = null;

        GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnObj != null)
        {
            currentRespawnPoint = respawnObj.transform;
            // Debug.Log($"Respawn point nalezen ve scéně {respawnObj.scene.name}");
        }
        else
        {
            // Debug.Log("V téhle scéně není žádný objekt s tagem 'Respawn'.");
        }
    }

    // ▶️ Obnoví hru
    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // ⏸️ Pauza
    void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Nouzový respawn – jen pokud v aktuální scéně existuje respawn point.
    /// </summary>
    public void ResetPlayerPosition()
    {
        if (player == null)
        {
            Debug.LogWarning("ResetPlayerPosition: Player nebyl nalezen (tag 'Player').");
            return;
        }

        if (currentRespawnPoint == null)
        {
            Debug.LogWarning("ResetPlayerPosition: V téhle scéně není žádný respawn point (tag 'Respawn').");
            return;
        }

        // Teleport pouze v rámci téhle scény
        player.position = currentRespawnPoint.position;
        // Pokud chceš, můžeš tu vypnout i rychlost, animaci apod.
    }

    // 🚪 Ukončení hry
    public void QuitGame()
    {
        Application.Quit();
    }
}
