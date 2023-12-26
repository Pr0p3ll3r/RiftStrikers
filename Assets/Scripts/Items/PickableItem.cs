using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items/PickableItem")]
public class PickableItem : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType = ItemType.Exp;
    public int value;
    public int weight;
}

public enum ItemType
{
    Health,
    Exp,
    Money
}
