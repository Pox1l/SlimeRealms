using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 🔥 NUTNÉ PRO COROUTINES (IEnumerator)

public class DeathUIManager : MonoBehaviour
{
    // Odkaz na celý objekt Canvasu
    public GameObject deadUICanvas;

    // 🔥 NOVÉ: Odkaz na CanvasGroup pro ovládání průhlednosti
    public CanvasGroup uiCanvasGroup;

    // 🔥 NOVÉ: Jak dlouho trvá, než se UI plně objeví (v sekundách)
    public float fadeDuration = 1.5f;

    public int sceneToReloadIndex = 1;

    void Start()
    {
        deadUICanvas.SetActive(false);
        PlayerStats.Instance.OnPlayerDied += ShowDeathUI;

        // Pokud zapomeneš přiřadit CanvasGroup v Inspectoru, zkusíme ho najít sami
        if (uiCanvasGroup == null && deadUICanvas != null)
        {
            uiCanvasGroup = deadUICanvas.GetComponent<CanvasGroup>();
        }
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowDeathUI;
        }
    }

    private void ShowDeathUI()
    {
        // 1. Aktivujeme objekt
        deadUICanvas.SetActive(true);

        // 2. Nastavíme ho jako průhledný a neklikatelný na začátek
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false; // Aby nešlo klikat, dokud se neobjeví
        }

        // 3. Zastavíme hru (fyziku a pohyb)
        Time.timeScale = 0f;

        // 4. Spustíme animaci (fade in)
        // Musíme použít StartCoroutine, protože chceme, aby se to dělo postupně
        StartCoroutine(FadeInUI());
    }

    // 🔥 TOTO JE TA ANIMACE
    IEnumerator FadeInUI()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            // Přičítáme čas. POZOR: Používáme unscaledDeltaTime, 
            // protože Time.timeScale je 0 (hra stojí), ale my chceme animovat.
            timer += Time.unscaledDeltaTime;

            // Vypočítáme průhlednost (číslo mezi 0 a 1)
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = alpha;
            }

            // Čekáme na další snímek
            yield return null;
        }

        // Na konci pojistíme, že je to plně viditelné a klikatelné
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 1f;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }

    public void RespawnButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToReloadIndex);
    }

    public void QuitButton()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}