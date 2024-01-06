using UnityEngine;

public class Item : ScriptableObject
{
    public string itemName;
    public int itemIconIndex;
    public bool isActive;
    public int maxLevel = 4;
    public int level = -1;

    public int GetLevel() { return level; }
    public void AddLevel() { level++; }

    public void Initialize()
    {
        level = -1;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Item other = (Item)obj;
        return itemName == other.itemName;
    }

    public override int GetHashCode()
    {
        return itemName.GetHashCode();
    }
}
