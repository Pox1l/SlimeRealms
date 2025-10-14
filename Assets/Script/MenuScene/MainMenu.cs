using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public int sceneIndex = 1;

    // Spust� hru (nahraje prvn� sc�nu, kterou si pojmenuje� nap�. "Game")
    public void PlayGame()
    {
        SceneManager.LoadScene(sceneIndex); // <- Zm�� n�zev sc�ny podle pot�eby
    }

    // Otev�e menu s mo�nostmi (options)
    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    // Vr�t� se z options zp�t do hlavn�ho menu
    public void BackToMenu()
    {
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Ukon�� hru
    public void QuitGame()
    {
        Debug.Log("Quit Game"); // Funguje jen v editoru
        Application.Quit();
    }
}
