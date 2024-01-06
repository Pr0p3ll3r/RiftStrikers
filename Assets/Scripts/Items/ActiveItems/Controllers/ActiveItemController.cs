using FishNet.Object;
using UnityEngine;

public abstract class ActiveItemController : NetworkBehaviour
{
    [HideInInspector] public ActiveItem activeItem;
    protected Transform playerTransform;
    protected float currentCooldown;

    protected virtual void Update()
    {
        if (!IsOwner) return;

        if (GameManager.Instance.currentState == GameState.Paused) return;

        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack();
        }
    }

    [ObserversRpc]
    public virtual void SetData(ActiveItem activeItem, Transform playerTransform)
    {
        this.activeItem = activeItem;
        this.playerTransform = playerTransform;
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
    }

    public void AddLevel()
    {
        activeItem.AddLevel();
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
    }

    protected virtual void Attack()
    {
        currentCooldown = activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown;
    }
}
