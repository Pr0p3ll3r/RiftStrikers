using FishNet.Object;
using UnityEngine;

public abstract class ActiveItemController : NetworkBehaviour
{
    protected ActiveItem activeItem;
    private float currentCooldown;

    protected virtual void Start()
    {
        currentCooldown = activeItem.GetCurrentLevel().cooldown;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack();
        }
    }

    public void SetData(ActiveItem activeItem)
    {
        this.activeItem = activeItem;
    }

    protected virtual void Attack()
    {
        currentCooldown = activeItem.GetCurrentLevel().cooldown;
    }
}
