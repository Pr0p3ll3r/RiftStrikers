using UnityEngine;

public class ForcefieldController : ActiveItemController
{
    private float currentRange;

    protected override void Start()
    {
        base.Start();
    }

    public override void SetData(ActiveItem activeItem)
    {
        base.SetData(activeItem);
        currentRange = activeItem.GetCurrentLevel().range;
        transform.localScale = new Vector3(currentRange, 1, currentRange);
    }

    protected override void Attack()
    {
        base.Attack();
        if(currentRange != activeItem.GetCurrentLevel().range)
        {
            currentRange = activeItem.GetCurrentLevel().range;
            transform.localScale = new Vector3(currentRange, 1, currentRange);
        }        
        Enemy[] enemiesInRange = GameManager.Instance.GetEnemiesInRange(transform.position, currentRange * Player.Instance.currentAttackRange);
        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
        }
    }
}
