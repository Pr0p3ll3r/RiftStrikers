using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class BuckyballController : ActiveItemController
{
    [SerializeField] private GameObject ballPrefab;

    protected override void Attack()
    {
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            SpawnServer(Owner);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServer(NetworkConnection Owner)
    {
        GameObject spawnedBall = Instantiate(ballPrefab, transform.position, ballPrefab.transform.rotation);
        Spawn(spawnedBall, Owner);
        spawnedBall.GetComponent<BuckyballBehaviour>().SetProjectileRpc(activeItem);
    }
}
