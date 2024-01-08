using FishNet.Object;
using System.Collections;
using UnityEngine;

public class BuckyballBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Rigidbody rb;
    private Vector3 lastVelocity;
    private float curSpeed;

    [ObserversRpc]
    public void SetProjectileRpc(ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {           
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            rb.velocity = activeItem.GetCurrentLevel().speed * Player.Instance.currentProjectileSpeed * randomDirection;
        }         
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void LateUpdate()
    {
        if (IsOwner)
            lastVelocity = rb.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer && IsClientInitialized)
        {
            if (collision.transform.root.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
            }
        }
        if (IsOwner)
        {
            curSpeed = lastVelocity.magnitude;
            Vector3 direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
            rb.velocity = direction * Mathf.Max(curSpeed, 0);
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        Despawn(gameObject);
    }
}
