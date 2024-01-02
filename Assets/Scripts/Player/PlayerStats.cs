using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [field: SerializeField]
    public float MaxHealth { get; private set; }
    [field: SerializeField]
    public float HealthRecovery { get; private set; }
    [field: SerializeField]
    public float DamageReduction { get; private set; }
    [field: SerializeField]
    public float MoveSpeed { get; private set; }
    [field: SerializeField]
    public float Damage { get; private set; }
    [field: SerializeField]
    public float AttackRange { get; private set; }
    [field: SerializeField]
    public float ProjectileSpeed { get; private set; }
    [field: SerializeField]
    public float AttackDuration { get; private set; }
    [field: SerializeField]
    public float AttackCooldown { get; private set; }
    [field: SerializeField]
    public float ExpGain { get; private set; }
    [field: SerializeField]
    public float MoneyGain { get; private set; }
    [field: SerializeField]
    public float LootRange { get; private set; }
}
