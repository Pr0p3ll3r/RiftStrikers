using FishNet.Object;
using System.Collections;
using UnityEngine;

public class SawbladeBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Transform playerTransform;
    private float angle;

    [ObserversRpc]
    public void SetProjectileRpc(ActiveItem activeItem, float angle, Transform playerTransform)
    {
        this.activeItem = activeItem;
        this.angle = angle;
        this.playerTransform = playerTransform;
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (GameManager.Instance.currentState == GameState.Paused) return;

        RotateSawblade();
    }

    private void RotateSawblade()
    {
        transform.Rotate(Vector3.up, activeItem.GetCurrentLevel().speed * Time.deltaTime);

        Vector3 desiredPosition = playerTransform.position + (Quaternion.Euler(0f, transform.eulerAngles.y + angle, 0f) * Vector3.forward * activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange);
        transform.position = desiredPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && IsClientInitialized && other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            if (enemy.CanBeDamagedBySawblade <= 0)
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                enemy.CanBeDamagedBySawblade = activeItem.GetCurrentLevel().hitDelay;
            }
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        Despawn(gameObject);
    }

}
