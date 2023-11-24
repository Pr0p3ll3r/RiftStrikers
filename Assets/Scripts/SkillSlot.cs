using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void SetSlot(Skill skill)
    {
        nameText.text = skill.skillName;
        iconImage.sprite = Database.GetSkillIcon(skill.skillIconIndex);
        levelText.text = skill.GetLevel().ToString();
        descriptionText.text = skill.levels[skill.GetLevel()].description;
    }
}
