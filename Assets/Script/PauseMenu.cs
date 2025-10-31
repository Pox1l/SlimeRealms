using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // ⚙️ Přiřaď GameObject panelu menu (Canvas nebo Panel)
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // ▶️ Obnoví hru
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // znovu spustí čas
        isPaused = false;
    }

    // ⏸️ Pozastaví hru
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // zastaví čas
        isPaused = true;
    }

    // 🚪 Ukončí hru
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

    }
}
