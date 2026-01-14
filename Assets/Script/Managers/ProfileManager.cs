using UnityEngine;
using System.IO;

public static class ProfileManager // 🔥 Všimni si slova "static"
{
    // Tato proměnná zůstane v paměti celou dobu běhu hry, i při změně scén
    public static string CurrentProfileID = "Slot_1";

    public static string GetSavePath(string fileName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, CurrentProfileID);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        return Path.Combine(folderPath, fileName);
    }
}