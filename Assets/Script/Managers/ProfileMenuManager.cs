using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ProfileMenuManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Dropdown profilDropdown;

    // 🔥 LevelLoader už tu nepotřebujeme, vyřešíme to v tlačítku

    private void Start()
    {
        NacistSloty();
        SetProfileID(); // Nastavíme výchozí hned při startu
        profilDropdown.onValueChanged.AddListener(delegate { SetProfileID(); });
    }

    // Tuhle funkci zavoláme z tlačítka
    public void SetProfileID()
    {
        int index = profilDropdown.value;
        ProfileManager.CurrentProfileID = "Slot_" + (index + 1);
        Debug.Log($"✅ Profil nastaven: {ProfileManager.CurrentProfileID}");
    }

    void NacistSloty()
    {
        profilDropdown.ClearOptions();
        List<string> moznosti = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            string id = "Slot_" + (i + 1);
            string cesta = Path.Combine(Application.persistentDataPath, id);

            if (Directory.Exists(cesta)) moznosti.Add($"Profil {i + 1} (Saved)");
            else moznosti.Add($"Profil {i + 1} (Empty)");
        }
        profilDropdown.AddOptions(moznosti);
    }
}