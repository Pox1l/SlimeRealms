using UnityEngine;
using System.IO;

public class TutorialSaveSystem : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        // Cestu si vezme z ProfileManageru
        savePath = ProfileManager.GetSavePath("tutorial_progress.json");
    }

    public void Save(TutorialData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("💾 Tutorial uložen.");
    }

    public TutorialData Load()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);

                // 1. Zkusíme převést JSON na data
                TutorialData data = JsonUtility.FromJson<TutorialData>(json);

                // 2. 🔥 POJISTKA: Pokud byl soubor prázdný, data budou null.
                // V tom případě vrátíme nový čistý objekt, ne null.
                if (data == null)
                {
                    Debug.LogWarning("Soubor existuje, ale byl prázdný. Vytvářím nová data.");
                    return new TutorialData();
                }

                return data;
            }
            catch
            {
                // 3. Toto chytí situaci, kdy jsou v souboru nesmyslné znaky (poškozený JSON)
                Debug.LogWarning("Chyba čtení JSONu (poškozený soubor), vytvářím nový.");
                return new TutorialData();
            }
        }

        // 4. Soubor vůbec neexistuje
        return new TutorialData();
    }

    [ContextMenu("Smazat Save")]
    public void DeleteSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        Debug.Log("Tutorial save smazán.");
    }
}