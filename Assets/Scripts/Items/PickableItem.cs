using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "HumanSurvivors/Items/PickableItem")]
public class PickableItem : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType = ItemType.Exp;
}

public enum ItemType
{
    Health,
    Exp,
    Money
}
