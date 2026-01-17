using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using System.Collections.Generic;

public class GlobalUISounds : MonoBehaviour
{
    public static GlobalUISounds instance;
    public EventReference clickSound;

    [Header("Settings")]
    public float minTimeBetweenSounds = 0.1f;
    private float lastPlayTime;

    // Seznam už napojených tlaèítek, abychom je nenapojovali 2x
    private HashSet<int> hookedButtons = new HashSet<int>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Pøi nové scénì vyèistíme seznam a najdeme tlaèítka
        hookedButtons.Clear();
        FindAndHookButtons();
    }

    // Tuhle funkci zavolej z jiného skriptu (tøeba PauseMenu), když otevøeš nové okno!
    public void RefreshButtons()
    {
        FindAndHookButtons();
    }

    void FindAndHookButtons()
    {
        // Trik: Resources.FindObjectsOfTypeAll najde úplnì všechno v pamìti (i vypnuté)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button btn in allButtons)
        {
            // 1. Ignorujeme tlaèítka, která jsou jen v projektu (prefabs) a ne ve høe
            if (btn.gameObject.scene.rootCount == 0) continue;

            //Debug.Log("Našel jsem tlaèítko: " + btn.name);
            // 2. Zkontrolujeme, jestli už jsme ho nenapojili (aby nehrálo 2x)
            int id = btn.GetInstanceID();
            if (hookedButtons.Contains(id)) continue;

            // 3. Napojíme zvuk
            btn.onClick.AddListener(() => PlayClick());
            hookedButtons.Add(id);
        }
    }

    public void PlayClick()
    {
        // Pøehrát jen pokud ubìhl èas (anti-spam)
        if (Time.unscaledTime - lastPlayTime > minTimeBetweenSounds && !clickSound.IsNull)
        {
            RuntimeManager.PlayOneShot(clickSound);
            lastPlayTime = Time.unscaledTime;
        }
    }

    // Speciální metoda pro Play tlaèítko (viz níže)
    public void PlayClickAndLoad(string sceneName)
    {
        StartCoroutine(LoadSceneWithDelay(sceneName));
    }

    System.Collections.IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // 1. Zahraj zvuk
        PlayClick();

        // 2. Poèkej chvilku (tøeba 0.2 sekundy), aby zvuk stihl zaèít
        yield return new WaitForSecondsRealtime(0.2f);

        // 3. Teprve teï naèti scénu
        SceneManager.LoadScene(sceneName);
    }
}