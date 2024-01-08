using FishNet.Object;
using System.Collections;
using UnityEngine;

public class BoomerangBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Rigidbody rb;
    private Vector3 initialDirection;
    private float returnTimer;
    private bool isReturning = false;

    [ObserversRpc]
    public void SetProjectileRpc(Vector3 closestEnemyPosition, ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        rb = GetComponent<Rigidbody>();
        initialDirection = (closestEnemyPosition - transform.position).normalized;
        returnTimer = activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration / 2;
        if (IsOwner)
        {
            rb.velocity = activeItem.GetCurrentLevel().speed * Player.Instance.currentProjectileSpeed * initialDirection;
        }
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void Update()
    {
        if (!IsOwner) return;

        transform.Rotate(Vector3.right, 100f * Time.deltaTime);

        if (returnTimer > 0)
            returnTimer -= Time.deltaTime;
        else if (!isReturning)
        {
            isReturning = true;
            rb.velocity = -rb.velocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsClientInitialized)
        {
            if (other.transform.root.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            }
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        Despawn(gameObject);
    }
}

