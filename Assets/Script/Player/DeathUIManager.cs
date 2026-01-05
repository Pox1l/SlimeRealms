using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deadUICanvas;
    public CanvasGroup uiCanvasGroup;

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    void Start()
    {
        // 1. Kontrola, zda máme přiřazené UI
        if (deadUICanvas == null)
        {
            Debug.LogError("CHYBA: Není přiřazen 'deadUICanvas' v Inspectoru!");
            return;
        }

        // 2. Automatické získání CanvasGroup, pokud chybí
        if (uiCanvasGroup == null)
        {
            uiCanvasGroup = deadUICanvas.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null)
            {
                // Pokud tam není, přidáme ho (pojistka)
                uiCanvasGroup = deadUICanvas.AddComponent<CanvasGroup>();
            }
        }

        // 3. Vypnutí UI na startu
        deadUICanvas.SetActive(false);

        // 4. Bezpečné přihlášení k eventu (čekáme, až bude PlayerStats existovat)
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied += ShowDeathUI;
        }
        else
        {
            Debug.LogError("POZOR: PlayerStats.Instance neexistuje v době Startu DeathUI! UI se nezobrazí.");
            // Zde by se dal použít Coroutine na čekání, ale pro teď stačí výpis chyby
        }
    }

    private void OnDestroy()
    {
        // Bezpečné odhlášení
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowDeathUI;
        }
    }

    private void ShowDeathUI()
    {
        Debug.Log("Zobrazuji Death UI"); // Kontrolní výpis

        deadUICanvas.SetActive(true);

        // Reset alpha a nastavení interakce
        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = true; // Aby nešlo klikat skrz

        // Zastavení času
        Time.timeScale = 0f;

        StartCoroutine(FadeInUI());
    }

    IEnumerator FadeInUI()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            // Používáme unscaledDeltaTime, protože timeScale je 0
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            uiCanvasGroup.alpha = alpha;
            yield return null;
        }

        uiCanvasGroup.alpha = 1f;
        uiCanvasGroup.interactable = true;

        // Odblokování kurzoru (pokud ho ve hře skrýváš)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RespawnButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitButton()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}