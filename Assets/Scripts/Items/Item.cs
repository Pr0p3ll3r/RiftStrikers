using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "HumanSurvivors/Items/Item")]
public class Item : ScriptableObject 
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType = ItemType.Default;
    [TextArea(4, 6)] public string description;

    public virtual void Initialize()
    {

    }

    public virtual Item GetCopy()
    {
        return this;
    }
}

public enum ItemType
{
    Default,
    Equipment,
    Health
}
