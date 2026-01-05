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
    [Tooltip("Minimální èas v sekundách, jak dlouho loading potrvá")]
    public float minLoadTime = 4f; // Zde nastav tøeba 4 sekundy

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        loadingScreen.SetActive(true);
        slider.value = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        // Smyèka bìží dokud není naèteno NEBO dokud neuplyne náš èas
        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            // 1. Spoèítáme progress podle reálného naèítání (0 až 1)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // 2. Spoèítáme progress podle našeho èasovaèe (0 až 1)
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);

            // 3. Použijeme tu MENŠÍ hodnotu. 
            // Pokud je hra naètená hned (real=1), brzdí to èasovaè (time).
            // Pokud by se hra sekla (real=0.5), bar nepobìží dopøedu, dokud se nenaète data.
            slider.value = Mathf.Min(realProgress, timeProgress);

            // Pokud je reálnì naèteno (0.9) A zároveò èasovaè dobìhl (1.0)
            if (operation.progress >= 0.9f && timeProgress >= 1f)
            {
                slider.value = 1f;
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}