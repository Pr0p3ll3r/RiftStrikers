using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [field: SerializeField]
    public float MaxHealth { get; set; }
    [field: SerializeField]
    public float HealthRecovery { get; set; }
    [field: SerializeField]
    public float Armor { get; set; }
    [field: SerializeField]
    public float MoveSpeed { get; set; }
    [field: SerializeField]
    public float Damage { get; set; }
    [field: SerializeField]
    public float ItemRange { get; set; }
    [field: SerializeField]
    public float ProjectileSpeed { get; set; }
    [field: SerializeField]
    public float ItemDuration { get; set; }
    [field: SerializeField]
    public float AttackCooldown { get; set; }
    [field: SerializeField]
    public float Luck { get; set; }
    [field: SerializeField]
    public float ExpGain { get; set; }
    [field: SerializeField]
    public float MoneyGain { get; set; }
    [field: SerializeField]
    public float LootRange { get; set; }
}
