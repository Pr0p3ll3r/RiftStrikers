using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "HumanSurvivors/Items/PickableItem")]
public class PickableItem : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType = ItemType.Exp;
    [Range(1, 100)] public int dropChance;
}

public enum ItemType
{
    Health,
    Exp,
    Money
}
