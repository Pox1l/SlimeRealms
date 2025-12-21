using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathUIManager : MonoBehaviour
{
    // Odkaz na celý objekt Canvasu
    public GameObject deadUICanvas;

    // Odkaz na CanvasGroup pro ovládání průhlednosti
    public CanvasGroup uiCanvasGroup;

    // Jak dlouho trvá, než se UI plně objeví (v sekundách)
    public float fadeDuration = 1.5f;

    // ❌ SMAZÁNO: public int sceneToReloadIndex = 1; (už to nepotřebujeme)

    void Start()
    {
        deadUICanvas.SetActive(false);
        PlayerStats.Instance.OnPlayerDied += ShowDeathUI;

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
        deadUICanvas.SetActive(true);

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
        }

        Time.timeScale = 0f;
        StartCoroutine(FadeInUI());
    }

    IEnumerator FadeInUI()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = alpha;
            }

            yield return null;
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 1f;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }

    public void RespawnButton()
    {
        // 1. Vrátíme čas do normálu
        Time.timeScale = 1f;

        // 2. 🔥 Získáme index aktuální scény a znovu ji načteme
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void QuitButton()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}