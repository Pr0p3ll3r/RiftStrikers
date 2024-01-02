using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyStats : ScriptableObject
{
    [field: SerializeField]
    public float Damage { get; private set; }
    [field: SerializeField] 
    public float AttackRange { get; private set; }
    [field: SerializeField]
    public float MoveSpeed { get; private set; }
    [field: SerializeField]
    public float MaxHealth { get; private set; }
    [field: SerializeField]
    public bool IsBoss { get; private set; }
    [field: SerializeField]
    public PickableItem[] Loot { get; private set; }
}
