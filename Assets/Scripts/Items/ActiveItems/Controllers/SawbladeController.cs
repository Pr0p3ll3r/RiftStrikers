using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class SawbladeController : ActiveItemController
{
    protected override void Attack()
    {
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown) + (activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        int amount = activeItem.GetCurrentLevel().projectile;
        float angleStep = 360f / amount;
        for (int i = 0; i < amount; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPosition = transform.position + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange);
            SpawnServer(spawnPosition, angle, Owner);
        }
    }

    [ObserversRpc]
    public override void SetData(ActiveItem activeItem, Transform playerTransform)
    {
        this.activeItem = activeItem;
        this.playerTransform = playerTransform;
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown) + (activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
    }

    public override void AddLevel()
    {
        activeItem.AddLevel();
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown) + (activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServer(Vector3 spawnPosition, float angle, NetworkConnection Owner)
    {
        GameObject sawblade = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Spawn(sawblade, Owner);
        sawblade.GetComponent<SawbladeBehaviour>().SetProjectileRpc(activeItem, angle, playerTransform);
    }
}
