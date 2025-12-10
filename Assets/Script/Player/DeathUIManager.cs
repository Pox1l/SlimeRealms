using UnityEngine;
using UnityEngine.SceneManagement; // 🔥 NUTNÉ PRO NAČÍTÁNÍ SCÉN

public class DeathUIManager : MonoBehaviour
{
    // Odkaz na váš Canvas DeadUI (přetáhnete v Inspectoru)
    public GameObject deadUICanvas;

    // 🔥 NOVÁ PROMĚNNÁ: Index scény, kterou chcete znovu načíst
    public int sceneToReloadIndex = 1;

    void Start()
    {
        // Zajistí, že UI je na začátku hry skryté
        deadUICanvas.SetActive(false);

        // Přihlásí se k odběru události smrti hráče
        PlayerStats.Instance.OnPlayerDied += ShowDeathUI;
    }

    private void OnDestroy()
    {
        // Odhlášení při zničení objektu je důležité
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowDeathUI;
        }
    }

    // Tato metoda se zavolá, když hráč zemře
    private void ShowDeathUI()
    {
        deadUICanvas.SetActive(true);
        // Zastaví hru, aby hráč nemohl hýbat pozadím
        Time.timeScale = 0f;
    }

    // --- FUNKCE PRO TLAČÍTKA ---

    /// <summary>
    /// Znovu načte scénu (Respawn/Restart).
    /// </summary>
    public void RespawnButton() // 🔥 PŘEJMENOVÁNO Z RestartGame
    {
        // 1. Obnovíme čas
        Time.timeScale = 1f;

        // 2. Načteme scénu s indexem 1 (musíte ji mít přidanou v Build Settings)
        // Použijte SceneManager.GetActiveScene().buildIndex pro načtení aktuální scény.
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(sceneToReloadIndex);
    }

    /// <summary>
    /// Ukončí aplikaci.
    /// </summary>
    public void QuitButton()
    {
        Debug.Log("QUIT GAME");
        // Funguje jen ve buildnuté aplikaci, v Editoru se ignoruje.
        Application.Quit();
    }
}