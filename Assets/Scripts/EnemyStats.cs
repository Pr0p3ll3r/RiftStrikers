using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyStats : ScriptableObject
{
    public float damage;
    public float attackRange;
    public float moveSpeed;
    public float maxHealth;
    public bool isBoss;
    public PickableItem[] loot;
}
