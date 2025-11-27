using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    public SkillSO skillData;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI levelText;

    private SkillTreeManager manager;

    void Start()
    {
        manager = FindObjectOfType<SkillTreeManager>();

        if (skillData != null)
        {
            if (iconImage != null) iconImage.sprite = skillData.icon;
            UpdateSlotVisuals();
        }

        btn.onClick.AddListener(() => manager.SelectSkill(this));
    }

    public void UpdateSlotVisuals()
    {
        if (skillData != null && levelText != null)
        {
            levelText.text = $"{skillData.currentLevel}/{skillData.MaxLevel}";
        }
    }
}