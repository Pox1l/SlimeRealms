using UnityEngine;

[CreateAssetMenu(menuName = "SkillTree/Skill")]

public class SkillSO : ScriptableObject
{
    public int itemID;   
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
}



