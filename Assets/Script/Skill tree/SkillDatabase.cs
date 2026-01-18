using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SkillDatabase : MonoBehaviour
{
    public static SkillDatabase Instance;

    [Header("Všechny Skilly")]
    public List<SkillSO> allSkills; // 🔥 Sem přetáhni všechny SkillSO

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);

            // ZMĚNA ZDE:
            savePath = ProfileManager.GetSavePath("skills_save.json");

            ResetAllSkills();
            LoadSkills();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetAllSkills()
    {
        foreach (var skill in allSkills)
        {
            skill.currentLevel = 0;
        }
    }

    public bool TryUpgradeSkill(SkillSO skill)
    {
        if (skill == null) return false;
        if (skill.currentLevel >= skill.MaxLevel) return false;

        // 1. Cena
        List<Requirement> cost = skill.levels[skill.currentLevel].cost;

        // 2. Kontrola itemů
        foreach (var req in cost)
        {
            if (InventoryManager.Instance.GetTotalItemCount(req.item) < req.amount)
            {
                Debug.Log("Nedostatek surovin!");
                return false;
            }
        }

        // 3. Odečíst itemy
        foreach (var req in cost)
        {
            InventoryManager.Instance.RemoveItem(req.item, req.amount);
        }

        // 4. Level Up
        skill.currentLevel++;

        // 5. Přepočítat hráče
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.RecalculateStats();
        }

        // 6. Uložit do JSON
        SaveSkills();

        return true;
    }

    // --- SAVE SYSTEM ---

    [System.Serializable]
    class SkillSaveData { 
        public int id;
        public string skillName;
        public int level;
    }
    [System.Serializable]
    class SaveList { public List<SkillSaveData> skills = new List<SkillSaveData>(); }

    public void SaveSkills()
    {
        SaveList saveList = new SaveList();
        foreach (var skill in allSkills)
            saveList.skills.Add(new SkillSaveData {
                id = skill.id,
                skillName = skill.skillName,
                level = skill.currentLevel
            });

        File.WriteAllText(savePath, JsonUtility.ToJson(saveList, true));
        Debug.Log($"💾 Skills uloženy do: {savePath}");
    }

    public void LoadSkills()
    {
        // 1. Pokud soubor vůbec neexistuje, rovnou ho vytvoříme s aktuálními daty (vše na 0)
        if (!File.Exists(savePath))
        {
            SaveSkills(); // 🔥 Vytvoří json hned při prvním startu
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);

            // 2. Pokud je soubor existuje, ale je prázdný
            if (string.IsNullOrEmpty(json))
            {
                SaveSkills(); // 🔥 Opraví prázdný soubor
                return;
            }

            SaveList loadedData = JsonUtility.FromJson<SaveList>(json);

            if (loadedData == null || loadedData.skills == null)
            {
                SaveSkills(); // 🔥 Opraví poškozená data
                return;
            }

            foreach (var saved in loadedData.skills)
            {
                var skill = allSkills.FirstOrDefault(s => s.id == saved.id);
                if (skill != null) skill.currentLevel = saved.level;
            }

            // Přepočet statistik po načtení
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.RecalculateStats(false, false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Chyba save file: {ex.Message}");
            // 3. Při kritické chybě resetujeme a uložíme čistý stav
            ResetAllSkills();
            SaveSkills(); // 🔥 Vytvoří nový soubor místo toho rozbitého
        }
    }
}