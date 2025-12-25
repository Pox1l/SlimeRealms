using UnityEngine;
using UnityEngine.UI; // 🔥 Nutné pro Toggle (Checkbox)
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    [Header("UI Prvky")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;
    public Toggle vsyncToggle; // 🔥 Sem přetáhni nový Checkbox

    void Start()
    {
        // 1. NAČTENÍ DAT
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        int savedFpsIndex = PlayerPrefs.GetInt("FpsIndex", 1);
        int savedVsync = PlayerPrefs.GetInt("VSyncEnabled", 0); // 0 = Vypnuto, 1 = Zapnuto

        // 2. AKTUALIZACE UI
        resolutionDropdown.value = savedResIndex;
        fpsDropdown.value = savedFpsIndex;

        // Nastavíme checkbox podle uložené hodnoty (1 == true, 0 == false)
        vsyncToggle.isOn = (savedVsync == 1);

        // 3. APLIKACE NASTAVENÍ (vše najednou)
        ChangeResolution(savedResIndex);
        RefreshFrameRateLogic(); // Speciální funkce, která řeší konflikt FPS vs VSync
    }

    // --- ROZLIŠENÍ ---
    public void ChangeResolution(int index)
    {
        if (index == 0) Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        else if (index == 1) Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    // --- FPS LIMIT (Volá se z Dropdownu) ---
    public void ChangeFPS(int index)
    {
        PlayerPrefs.SetInt("FpsIndex", index);
        PlayerPrefs.Save();

        // Po změně v dropdownu musíme přepočítat logiku (zohlednit VSync)
        RefreshFrameRateLogic();
    }

    // --- VSYNC (Volá se z Toggle Checkboxu) ---
    public void ToggleVSync(bool isEnabled)
    {
        PlayerPrefs.SetInt("VSyncEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // Po kliknutí na checkbox musíme přepočítat logiku
        RefreshFrameRateLogic();
    }

    // --- HLAVNÍ LOGIKA PRO FPS A VSYNC ---
    // Tuhle funkci voláme interně, aby se VSync a FPS nehádaly
    private void RefreshFrameRateLogic()
    {
        // Pokud je Checkbox zaškrtnutý -> Zapneme VSync
        if (vsyncToggle.isOn)
        {
            QualitySettings.vSyncCount = 1; // Zapnuto (synchronizace s monitorem)
            Application.targetFrameRate = -1; // FPS limit necháme na monitoru
        }
        else
        {
            // Pokud je Checkbox vypnutý -> Vypneme VSync a řídíme se Dropdownem
            QualitySettings.vSyncCount = 0;

            int index = fpsDropdown.value;
            if (index == 0) Application.targetFrameRate = 30;
            else if (index == 1) Application.targetFrameRate = 60;
            else if (index == 2) Application.targetFrameRate = -1;
        }
    }
}