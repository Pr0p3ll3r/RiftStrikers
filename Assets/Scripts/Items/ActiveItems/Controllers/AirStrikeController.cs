using System.Collections.Generic;
using UnityEngine;

public class AirStrikeController : ActiveItemController
{
    [SerializeField] private GameObject laserPrefab;

    protected override void Attack()
    {
        base.Attack();
        List<Enemy> tempList = new List<Enemy>(GameManager.Instance.Enemies);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(transform.position, activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange, tempList);
            if (closestEnemy != null)
            {
                GameObject spawnedLaser = Instantiate(laserPrefab, closestEnemy.transform.position, laserPrefab.transform.rotation);
                Spawn(spawnedLaser);
                spawnedLaser.GetComponent<AirStrikeBehaviour>().SetProjectile(activeItem);
                if (closestEnemy.TryGetComponent<Enemy>(out var enemyComponent))
                {
                    enemyComponent.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                    tempList.Remove(enemyComponent);
                }
            }
        }
    }
}
