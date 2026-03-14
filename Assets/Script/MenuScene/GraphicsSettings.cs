using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GraphicsSettings : MonoBehaviour
{
    [Header("UI Prvky - Rozlišení (Podle videa)")]
    public TMP_Text resolutionLabel;
    public List<ResItem> resolutions = new List<ResItem>();
    private int selectedResolution;

    [Header("UI Prvky - FPS")]
    public TMP_Text fpsLabel; // Nahrazeno: Text místo Dropdownu
    private int selectedFpsIndex;
    private int[] fpsOptions = { 30, 60, -1 }; // -1 = Unlimited
    private string[] fpsDisplayNames = { "30", "60", "Unlimited" };

    [Header("UI Prvky - Ostatní")]
    public Toggle vsyncToggle;
    public Toggle fullscreenToggle;

    void Start()
    {
        fullscreenToggle.isOn = Screen.fullScreen;

        int savedFpsIndex = PlayerPrefs.GetInt("FpsIndex", 1);
        int savedVsync = PlayerPrefs.GetInt("VSyncEnabled", 0);

        // Nastavení FPS při startu
        selectedFpsIndex = savedFpsIndex;
        UpdateFpsLabel();

        vsyncToggle.isOn = (savedVsync == 1);

        bool foundRes = false;
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                foundRes = true;
                selectedResolution = i;
                UpdateResLabel();
            }
        }

        if (!foundRes)
        {
            ResItem newRes = new ResItem();
            newRes.horizontal = Screen.width;
            newRes.vertical = Screen.height;
            resolutions.Add(newRes);
            selectedResolution = resolutions.Count - 1;
            UpdateResLabel();
        }

        RefreshFrameRateLogic();
    }

    // --- ROZLIŠENÍ ---
    public void ResLeft()
    {
        selectedResolution--;
        if (selectedResolution < 0) selectedResolution = 0;
        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1) selectedResolution = resolutions.Count - 1;
        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
    }

    public void ApplyGraphics()
    {
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("FullscreenEnabled", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, isFullscreen);
        PlayerPrefs.SetInt("FullscreenEnabled", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- FPS ---
    public void FpsLeft()
    {
        selectedFpsIndex--;
        if (selectedFpsIndex < 0) selectedFpsIndex = 0;
        UpdateFpsLabel();
        ApplyFPS();
    }

    public void FpsRight()
    {
        selectedFpsIndex++;
        if (selectedFpsIndex > fpsOptions.Length - 1) selectedFpsIndex = fpsOptions.Length - 1;
        UpdateFpsLabel();
        ApplyFPS();
    }

    public void UpdateFpsLabel()
    {
        fpsLabel.text = fpsDisplayNames[selectedFpsIndex];
    }

    public void ApplyFPS()
    {
        PlayerPrefs.SetInt("FpsIndex", selectedFpsIndex);
        PlayerPrefs.Save();
        RefreshFrameRateLogic();
    }

    public void ToggleVSync(bool isEnabled)
    {
        PlayerPrefs.SetInt("VSyncEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        RefreshFrameRateLogic();
    }

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
            Application.targetFrameRate = fpsOptions[selectedFpsIndex];
        }
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}