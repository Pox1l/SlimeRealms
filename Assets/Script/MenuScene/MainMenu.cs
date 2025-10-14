using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public int sceneIndex = 1;

    // Spustí hru (nahraje první scénu, kterou si pojmenuješ napø. "Game")
    public void PlayGame()
    {
        SceneManager.LoadScene(sceneIndex); // <- Zmìò název scény podle potøeby
    }

    // Otevøe menu s možnostmi (options)
    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    // Vrátí se z options zpìt do hlavního menu
    public void BackToMenu()
    {
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Ukonèí hru
    public void QuitGame()
    {
        Debug.Log("Quit Game"); // Funguje jen v editoru
        Application.Quit();
    }
}
