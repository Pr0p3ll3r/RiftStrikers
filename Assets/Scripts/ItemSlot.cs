using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void SetSlot(Item item)
    {
        nameText.text = item.itemName;
        iconImage.sprite = Database.GetItemIcon(item.itemIconIndex);
        levelText.text = (item.GetLevel() + 2).ToString();
        if (item is ActiveItem activeItem)
        {
            descriptionText.text = activeItem.GetNextLevelDescription();
        }
        //else if (item is PassiveItem passiveItem)
        //{
        //    descriptionText.text = passiveItem.GetNextLevelDescription();
        //}
    }
}
