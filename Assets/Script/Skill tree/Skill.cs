using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public string description;
    public Sprite icon;

    [Header("Unlock requirements")]
    public string requiredItemID;   // napø. "FireRune" nebo "SkillGem"
    public int requiredItemCount = 1;

    [Header("State")]
    public bool unlocked = false;
    public Skill[] prerequisites;

    public void Unlock()
    {
        unlocked = true;
    }
}
