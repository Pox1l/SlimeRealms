using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events; // 🔥 Důležité: Přidáno pro UnityEvent

public class LevelLoader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject loadingScreen;
    public Slider slider;

    [Header("Nastavení")]
    public float minLoadTime = 4f;

    [Header("Co se má stát při startu (Profil, Zvuk)")]
    public UnityEvent OnLoadStart; // 🔥 SEM v inspektoru přetáhneš Profil a Zvuk

    public void LoadLevel(int sceneIndex)
    {
        // 1. HNED zapneme Loading Screen
        loadingScreen.SetActive(true);
        Time.timeScale = 1f;

        // 2. Spustíme coroutinu, která se postará o pořadí
        StartCoroutine(LoadSequence(sceneIndex));
    }

    IEnumerator LoadSequence(int sceneIndex)
    {
        // 🔥 Čekáme 1 snímek – to donutí Unity vykreslit Loading Screen na monitor
        yield return null;

        // 3. Teď spustíme Profil a Zvuk. I když se tady hra sekne, hráč už vidí Loading Screen!
        OnLoadStart.Invoke();

        // Pro jistotu počkáme ještě do konce snímku, aby se zvuk chytil
        yield return new WaitForEndOfFrame();

        // 4. Až teď začneme načítat scénu
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