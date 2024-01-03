using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI description;

    public void SetSlot(Item item)
    {
        itemName.text = item.itemName;
        iconImage.sprite = Database.GetItemIcon(item.itemIconIndex);
        level.text = (item.GetLevel() + 2).ToString();
        type.text = item.isActive ? "Active" : "Passive";
        if (item is ActiveItem activeItem)
        {
            description.text = activeItem.GetNextLevelDescription();
        }
        else if (item is PassiveItem passiveItem)
        {
            description.text = passiveItem.description;
        }
    }
}
