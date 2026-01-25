using UnityEngine;
using System.Collections.Generic;

public enum SkillType { None, Health, Damage, Speed, Stamina, Defense }

[CreateAssetMenu(menuName = "Data/Skill")]
public class SkillSO : ScriptableObject
{
    public int id;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Efekt Skillu")]
    public SkillType type;
    public int valuePerLevel; // 🔥 Změněno na int (celá čísla)

    [Header("Leveling")]
    public int currentLevel;
    public List<SkillStage> levels;

    public int MaxLevel => levels.Count;

    public int GetTotalBonus() // 🔥 Změněno na int
    {
        return currentLevel * valuePerLevel;
    }
}

[System.Serializable]
public struct SkillStage
{
    public List<Requirement> cost;
}

[System.Serializable]
public struct Requirement
{
    public ItemSO item;
    public int amount;
}