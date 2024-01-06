using UnityEngine;

public class ForcefieldController : ActiveItemController
{
    private float currentRange;

    public override void SetData(ActiveItem activeItem)
    {
        base.SetData(activeItem);
        currentRange = activeItem.GetCurrentLevel().range;
        transform.localScale = new Vector3(currentRange, 1, currentRange);
    }

    protected override void Update()
    {
        if (currentRange != activeItem.GetCurrentLevel().range)
        {
            currentRange = activeItem.GetCurrentLevel().range;
            transform.localScale = new Vector3(currentRange, 1, currentRange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<Enemy>(out var enemy) && enemy.CanBeDamagedByForceField <= 0)
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            enemy.CanBeDamagedByForceField = activeItem.GetCurrentLevel().cooldown;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.TryGetComponent<Enemy>(out var enemy) && enemy.CanBeDamagedByForceField <= 0)
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            enemy.CanBeDamagedByForceField = activeItem.GetCurrentLevel().cooldown;
        }
    }
}
