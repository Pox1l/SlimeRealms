using UnityEngine;
using System.IO;

public class TutorialSaveSystem : MonoBehaviour
{
    [Header("Nastavení ukládání")]
    [Tooltip("Změň název pro každý tutoriál, např. 'tutorial_main.json' a 'tutorial_simple.json'")]
    public string saveFileName = "tutorial_progress.json";

    private string savePath;

    private void Awake()
    {
        savePath = ProfileManager.GetSavePath(saveFileName);
    }

    public void Save(TutorialData data)
    {
        string directoryPath = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Tutorial uložen do {saveFileName}.");
    }

    public TutorialData Load()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                TutorialData data = JsonUtility.FromJson<TutorialData>(json);

                if (data == null)
                {
                    return new TutorialData();
                }

                return data;
            }
            catch
            {
                Debug.LogWarning("Chyba čtení JSONu, vytvářím nový.");
                return new TutorialData();
            }
        }

        return new TutorialData();
    }

    [ContextMenu("Smazat Save")]
    public void DeleteSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        Debug.Log($"Tutorial save {saveFileName} smazán.");
    }
}