using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject loadingScreen;
    public Slider slider;

    [Header("Nastavení")]
    public float minLoadTime = 4f;

    public void LoadLevel(int sceneIndex)
    {
        // 🔥 OPRAVA: Musíme zajistit, že čas běží, jinak se loading zasekne!
        Time.timeScale = 1f;

        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        loadingScreen.SetActive(true);
        slider.value = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            // Přičítáme čas
            elapsedTime += Time.deltaTime;

            // Spočítáme progress (0 až 1)
            // operation.progress jde jen do 0.9, proto dělíme 0.9
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Spočítáme náš umělý časovač (0 až 1)
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);

            // Slider ukazuje tu MENŠÍ hodnotu (aby neproletěl hned na konec)
            slider.value = Mathf.Min(realProgress, timeProgress);

            // Pokud je hra načtená (0.9) A zároveň uběhl náš čas (1.0)
            if (operation.progress >= 0.9f && timeProgress >= 1f)
            {
                slider.value = 1f;
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}