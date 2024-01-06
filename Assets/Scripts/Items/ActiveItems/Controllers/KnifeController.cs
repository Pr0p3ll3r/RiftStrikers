using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : ActiveItemController
{
    [SerializeField] private GameObject knifePrefab;

    protected override void Attack()
    {
        base.Attack();
        List<Enemy> tempList = new List<Enemy>(GameManager.Instance.enemies);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(playerTransform.transform.position, activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange, tempList);
            if (closestEnemy != null)
            {
                SpawnServer(closestEnemy, Owner);
                if (closestEnemy.TryGetComponent<Enemy>(out var enemyComponent))
                {
                    tempList.Remove(enemyComponent);
                }
            }
        }      
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServer(GameObject closestEnemy, NetworkConnection Owner)
    {
        GameObject spawnedKnife = Instantiate(knifePrefab, transform.position, knifePrefab.transform.rotation);
        Spawn(spawnedKnife, Owner);
        spawnedKnife.GetComponent<KnifeBehaviour>().SetProjectileRpc(closestEnemy, activeItem);
    }
}
