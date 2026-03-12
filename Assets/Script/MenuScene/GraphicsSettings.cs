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

    [Header("UI Prvky - Ostatní")]
    public TMP_Dropdown fpsDropdown;
    public Toggle vsyncToggle;
    public Toggle fullscreenToggle;

    void Start()
    {
        fullscreenToggle.isOn = Screen.fullScreen;

        int savedFpsIndex = PlayerPrefs.GetInt("FpsIndex", 1);
        int savedVsync = PlayerPrefs.GetInt("VSyncEnabled", 0);
        fpsDropdown.value = savedFpsIndex;
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

    // --- TLAČÍTKO APPLY (Pro rozlišení) ---
    public void ApplyGraphics()
    {
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("FullscreenEnabled", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- FULLSCREEN CHECKBOX (Okamžitá reakce) ---
    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, isFullscreen);
        PlayerPrefs.SetInt("FullscreenEnabled", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeFPS(int index)
    {
        PlayerPrefs.SetInt("FpsIndex", index);
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
            int index = fpsDropdown.value;
            if (index == 0) Application.targetFrameRate = 30;
            else if (index == 1) Application.targetFrameRate = 60;
            else if (index == 2) Application.targetFrameRate = -1;
        }
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}