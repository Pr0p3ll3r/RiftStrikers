using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class AirStrikeController : ActiveItemController
{
    [SerializeField] private GameObject laserPrefab;

    protected override void Attack()
    {
        base.Attack();
        List<Enemy> tempList = new List<Enemy>(GameManager.Instance.enemies);
        Debug.Log(tempList.Count);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(playerTransform.transform.position, activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange, tempList);
            if (closestEnemy != null)
            {
                SpawnServer(closestEnemy.transform.position, Owner);
                if (closestEnemy.TryGetComponent<Enemy>(out var enemyComponent))
                {
                    enemyComponent.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                    tempList.Remove(enemyComponent);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServer(Vector3 spawnPosition, NetworkConnection Owner)
    {
        GameObject spawnedLaser = Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
        Spawn(spawnedLaser, Owner);
    }
}
