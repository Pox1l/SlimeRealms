using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class LevelLoader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject loadingScreen;
    public Slider slider;

    [Header("Náhodné Pozadí")]
    public Image backgroundImage;      // 🔥 NOVÉ: Sem přetáhni ten Image komponent, co má měnit obrázek
    public Sprite[] backgroundSprites; // 🔥 NOVÉ: Sem vlož ty 3 (nebo víc) obrázky

    [Header("Nastavení")]
    public float minLoadTime = 4f;

    [Header("Co se má stát při startu (Profil, Zvuk)")]
    public UnityEvent OnLoadStart;

    public void LoadLevel(int sceneIndex)
    {
        // 🔥 NOVÉ: Vybereme náhodný obrázek (pokud nějaké máme)
        if (backgroundSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            backgroundImage.sprite = backgroundSprites[randomIndex];
        }

        // 1. HNED zapneme Loading Screen (už s novým obrázkem)
        loadingScreen.SetActive(true);
        Time.timeScale = 1f;

        // 2. Spustíme sekvenci
        StartCoroutine(LoadSequence(sceneIndex));
    }

    IEnumerator LoadSequence(int sceneIndex)
    {
        yield return null;

        OnLoadStart.Invoke();

        yield return new WaitForEndOfFrame();

        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        slider.value = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);

            slider.value = Mathf.Min(realProgress, timeProgress);

            if (operation.progress >= 0.9f && timeProgress >= 1f)
            {
                slider.value = 1f;
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}