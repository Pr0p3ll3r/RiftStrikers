using FishNet.Object;
using System.Collections;
using UnityEngine;

public class SawbladeBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Transform playerTransform;
    private float angle;

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        Despawn(gameObject);
    }

    private void Update()
    {
        if (!IsOwner) return;

        RotateSawblade();
    }

    private void RotateSawblade()
    {
        transform.Rotate(Vector3.up, activeItem.GetCurrentLevel().speed * Time.deltaTime);

        Vector3 desiredPosition = playerTransform.position + (Quaternion.Euler(0f, transform.eulerAngles.y + angle, 0f) * Vector3.forward * activeItem.GetCurrentLevel().range);
        transform.position = desiredPosition;
    }

    [ObserversRpc]
    public void SetProjectileRpc(ActiveItem activeItem, float angle, Transform player)
    {
        this.activeItem = activeItem;
        this.angle = angle;
        playerTransform = player;
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
        }
    }
}
