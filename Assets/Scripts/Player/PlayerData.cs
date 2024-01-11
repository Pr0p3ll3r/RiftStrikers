public class PlayerData
{
    public string Name { get; set; }
    public int Money { get; set; }
    public float MaxHealth { get; set; }
    public float HealthRecovery { get; set; }
    public float DamageReduction { get; set; }
    public float MoveSpeed { get; private set; }
    public float Damage { get; set; }
    public float AttackRange { get; set; }
    public float ProjectileSpeed { get; set; }
    public float AttackDuration { get; set; }
    public float AttackCooldown { get; set; }
    public float ExpGain { get; set; }
    public float MoneyGain { get; set; }
    public float LootRange { get; set; }
}
