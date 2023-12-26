using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyStats : ScriptableObject
{
    public int damage;
    public float attackRange;
    public float moveSpeed;
    public int maxHealth;
    public bool isBoss;

    public int exp;
    public int money;
    public PickableItem[] loot;
}
