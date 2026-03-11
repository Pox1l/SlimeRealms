using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// TÉMA: Zachování Dropdownu s využitím dynamického listu (ResItem) a logiky z videa
public class GraphicsSettings : MonoBehaviour
{
    [Header("UI Prvky")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;
    public Toggle vsyncToggle;
    public Toggle fullscreenToggle;

    [Header("Nastavení Rozlišení")]
    public List<ResItem> resolutions = new List<ResItem>();

    void Start()
    {
        // 1. Z VIDEA: Vezme aktuální stav fullscreenu
        fullscreenToggle.isOn = Screen.fullScreen;

        // 2. Z VIDEA: Zkontroluje, jestli je aktuální rozlišení obrazovky v našem listu
        bool foundRes = false;
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Count; i++)
        {
            if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                foundRes = true;
                currentResIndex = i;
            }
        }

        // Pokud není, přidá ho na konec listu
        if (!foundRes)
        {
            ResItem newRes = new ResItem();
            newRes.horizontal = Screen.width;
            newRes.vertical = Screen.height;
            resolutions.Add(newRes);
            currentResIndex = resolutions.Count - 1;
        }

        // 3. AUTOMATICKÉ NAPLNĚNÍ DROPDOWNU PODLE LISTU
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Count; i++)
        {
            options.Add(resolutions[i].horizontal + " x " + resolutions[i].vertical);
        }
        resolutionDropdown.AddOptions(options);

        // 4. NAČTENÍ DAT A AKTUALIZACE UI
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        int savedFpsIndex = PlayerPrefs.GetInt("FpsIndex", 1);
        int savedVsync = PlayerPrefs.GetInt("VSyncEnabled", 0);

        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue(); // Aktualizuje text v UI

        fpsDropdown.value = savedFpsIndex;
        vsyncToggle.isOn = (savedVsync == 1);

        // 5. APLIKACE NASTAVENÍ
        ApplyResolution(savedResIndex, fullscreenToggle.isOn);
        RefreshFrameRateLogic();
    }

    // --- ROZLIŠENÍ ---
    public void ChangeResolution(int index)
    {
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();

        ApplyResolution(index, fullscreenToggle.isOn);
    }

    // --- FULLSCREEN ---
    public void ToggleFullscreen(bool isFullscreen)
    {
        PlayerPrefs.SetInt("FullscreenEnabled", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        ApplyResolution(resolutionDropdown.value, isFullscreen);
    }

    // Samotná okamžitá změna obrazovky pomocí listu z videa
    private void ApplyResolution(int index, bool isFullscreen)
    {
        // Pojistka, kdyby byl uložený index mimo rozsah listu
        if (index < 0 || index >= resolutions.Count) return;

        FullScreenMode mode = isFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

        Screen.fullScreen = isFullscreen;
        Screen.SetResolution(resolutions[index].horizontal, resolutions[index].vertical, mode);
    }

    // --- FPS LIMIT ---
    public void ChangeFPS(int index)
    {
        PlayerPrefs.SetInt("FpsIndex", index);
        PlayerPrefs.Save();
        RefreshFrameRateLogic();
    }

    // --- VSYNC ---
    public void ToggleVSync(bool isEnabled)
    {
        PlayerPrefs.SetInt("VSyncEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        RefreshFrameRateLogic();
    }

    // --- HLAVNÍ LOGIKA PRO FPS A VSYNC ---
    private void RefreshFrameRateLogic()
    {
        if (vsyncToggle.isOn)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;

            int index = fpsDropdown.value;
            if (index == 0) Application.targetFrameRate = 30;
            else if (index == 1) Application.targetFrameRate = 60;
            else if (index == 2) Application.targetFrameRate = -1;
        }
    }
}

// Třída z videa pro list rozlišení
[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}