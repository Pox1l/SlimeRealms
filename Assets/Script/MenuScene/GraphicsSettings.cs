using UnityEngine;
using TMPro; // Dùležité pro TextMeshPro Dropdowny

public class GraphicsSettings : MonoBehaviour
{
    [Header("Pøetáhni sem Dropdowny z Inspectoru")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;

    void Start()
    {
        // 1. NAÈTENÍ ULOŽENÝCH DAT
        // Pokud hra bìží poprvé, použije se defaultní hodnota (druhé èíslo v závorce)
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        int savedFpsIndex = PlayerPrefs.GetInt("FpsIndex", 1); // Defaultnì napø. 60 FPS (index 1)

        // 2. AKTUALIZACE VZHLEDU DROPDOWNÙ
        // Aby v menu svítilo to, co je reálnì nastavené
        resolutionDropdown.value = savedResIndex;
        fpsDropdown.value = savedFpsIndex;

        // 3. APLIKACE NASTAVENÍ
        // Hned pøi startu (nebo naètení scény) se grafika pøenastaví
        ChangeResolution(savedResIndex);
        ChangeFPS(savedFpsIndex);
    }

    // Tuto funkci propoj v Unity na Dropdown (Resolution) -> OnValueChanged
    public void ChangeResolution(int index)
    {
        // Pøíklad rozlišení - uprav si podle sebe
        if (index == 0) Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        else if (index == 1) Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

        // ULOŽIT VOLBU
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    // Tuto funkci propoj v Unity na Dropdown (FPS) -> OnValueChanged
    public void ChangeFPS(int index)
    {
        QualitySettings.vSyncCount = 0; // Nutné vypnout VSync pro manuální FPS

        if (index == 0) Application.targetFrameRate = 30;
        else if (index == 1) Application.targetFrameRate = 60;
        else if (index == 2) Application.targetFrameRate = -1; // Neomezeno

        // ULOŽIT VOLBU
        PlayerPrefs.SetInt("FpsIndex", index);
        PlayerPrefs.Save();
    }
}