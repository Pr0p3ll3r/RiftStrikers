using FishNet.Object;
using System.Collections;
using UnityEngine;

public class FireBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;

    [ObserversRpc]
    public void SetProjectileRpc(ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        float currentArea = activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange;
        transform.localScale = new Vector3(currentArea, currentArea, currentArea);       
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && IsClientInitialized && other.transform.root.TryGetComponent(out Enemy enemy))
        {
            if (enemy.CanBeDamagedByFire <= 0)
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                enemy.CanBeDamagedByFire = activeItem.GetCurrentLevel().hitDelay;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && IsClientInitialized && other.transform.root.TryGetComponent(out Enemy enemy))
        {
            if (enemy.CanBeDamagedByFire <= 0)
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                enemy.CanBeDamagedByFire = activeItem.GetCurrentLevel().hitDelay;
            }
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        Despawn(gameObject);
    }
}
