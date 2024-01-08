using FishNet.Object;
using UnityEngine;

public class BuckyballBehaviour : ProjectileBehaviour
{
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
            float currentArea = activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange;
            transform.localScale = new Vector3(currentArea, currentArea, currentArea);
        }         
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (GameManager.Instance.currentState == GameState.Paused) return;

        lastVelocity = rb.velocity;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (IsOwner)
        {
            curSpeed = lastVelocity.magnitude;
            Vector3 direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
            rb.velocity = direction * Mathf.Max(curSpeed, 0);
        }
    }
}
