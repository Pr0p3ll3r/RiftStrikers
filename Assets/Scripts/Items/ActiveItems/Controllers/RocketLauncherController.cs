using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherController : ActiveItemController
{
    [SerializeField] private GameObject rocketPrefab;

    protected override void Attack()
    {
        base.Attack();
        List<Enemy> tempList = new List<Enemy>(GameManager.Instance.enemies);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(playerTransform.position, activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange, tempList);
            if (closestEnemy != null)
            {
                SpawnServer(closestEnemy.transform.position, Owner);
                if (closestEnemy.TryGetComponent<Enemy>(out var enemyComponent))
                {
                    tempList.Remove(enemyComponent);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServer(Vector3 closestEnemyPosition, NetworkConnection Owner)
    {
        GameObject spawnedRocket = Instantiate(rocketPrefab, transform.position, rocketPrefab.transform.rotation);
        Spawn(spawnedRocket, Owner);
        spawnedRocket.GetComponent<RocketBehaviour>().SetProjectileRpc(closestEnemyPosition, activeItem);
    }
}
