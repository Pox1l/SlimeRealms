using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Potøebné pro práci s Button

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public int sceneIndex = 1;

    void Start()
    {
        // Automatické pøiøazení XBTN
        AssignBackButton();
    }

    private void AssignBackButton()
    {
        // Pokud optionsMenuPanel není pøiøazen, zkusíme ho najít pøes tag
        if (optionsMenuPanel == null)
        {
            GameObject settingsCanvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
            if (settingsCanvas != null)
            {
                // Najdeme SettingsMenu uvnitø Canvasu
                Transform menuTrans = settingsCanvas.transform.Find("SettingsMenu");
                if (menuTrans != null) optionsMenuPanel = menuTrans.gameObject;
            }
        }

        // Pokud máme panel, najdeme v nìm tlaèítko XBTN
        if (optionsMenuPanel != null)
        {
            Button xBtn = null;
            // Prohledá všechny potomky a hledá Button se jménem "XBTN"
            foreach (Button b in optionsMenuPanel.GetComponentsInChildren<Button>(true))
            {
                if (b.name == "XBTN")
                {
                    xBtn = b;
                    break;
                }
            }

            if (xBtn != null)
            {
                xBtn.onClick.RemoveAllListeners();
                xBtn.onClick.AddListener(BackToMenu);
            }
        }
    }

    // Spustí hru
    public void PlayGame()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Otevøe menu s možnostmi
    public void OpenOptions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(true);
    }

    // Vrátí se z options zpìt do hlavního menu
    public void BackToMenu()
    {
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // Ukonèí hru
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}