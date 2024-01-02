using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItem", menuName = "ScriptableObjects/Items/PassiveItem")]
public class PassiveItem : Item
{
    [field: SerializeField]
    public PassiveItemType ItemType { get; set; }
    [TextArea(4, 6)] public string description;
    public float multiplier;
}

public enum PassiveItemType
{
    MaxHealth,
    HealthRecovery,
    DamageReduction,
    MoveSpeed,
    Damage,
    AttackRange,
    ProjectileSpeed,
    AttackDuration,
    AttackCooldown,
    ExpGain,
    MoneyGain,
    LootRange
}