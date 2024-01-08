using FishNet.Object;
using UnityEngine;

public class ForcefieldController : ActiveItemController
{
    private float currentArea;

    [ObserversRpc]
    public override void SetData(ActiveItem activeItem, Transform playerTransform)
    {
        this.activeItem = activeItem;
        this.playerTransform = playerTransform;
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
        currentArea = activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange;
        transform.localScale = new Vector3(currentArea, 1, currentArea);
        transform.localPosition = new Vector3(0f, -0.9f, 0f);
    }

    [ObserversRpc]
    public override void AddLevel()
    {
        activeItem.AddLevel();
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
        currentArea = activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange;
        transform.localScale = new Vector3(currentArea, 1, currentArea);
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
