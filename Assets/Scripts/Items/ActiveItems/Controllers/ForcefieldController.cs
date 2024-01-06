using FishNet.Object;
using UnityEngine;

public class ForcefieldController : ActiveItemController
{
    private float currentRange;

    [ObserversRpc]
    public override void SetData(ActiveItem activeItem, Transform playerTransform)
    {
        this.activeItem = activeItem;
        this.playerTransform = playerTransform;
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
        currentRange = activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange;
        transform.localScale = new Vector3(currentRange, 1, currentRange);
    }

    protected override void Update()
    {
        if (!IsOwner) return;

        if (currentRange != activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange)
        {
            currentRange = activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange;
            transform.localScale = new Vector3(currentRange, 1, currentRange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && other.transform.root.TryGetComponent<Enemy>(out var enemy) && enemy.CanBeDamagedByForceField <= 0)
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            enemy.CanBeDamagedByForceField = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && other.transform.root.TryGetComponent<Enemy>(out var enemy) && enemy.CanBeDamagedByForceField <= 0)
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            enemy.CanBeDamagedByForceField = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
        }
    }
}
