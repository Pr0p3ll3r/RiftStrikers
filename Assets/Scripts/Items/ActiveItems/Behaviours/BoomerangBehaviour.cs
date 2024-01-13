using FishNet.Object;
using UnityEngine;

public class BoomerangBehaviour : ProjectileBehaviour
{
    private Vector3 initialDirection;
    private float returnTimer;
    private bool isReturning = false;

    [ObserversRpc]
    public void SetProjectileRpc(Vector3 closestEnemyPosition, ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            initialDirection = (closestEnemyPosition - transform.position).normalized;
            returnTimer = activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration / 2;
            rb.velocity = activeItem.GetCurrentLevel().speed * Player.Instance.currentProjectileSpeed * initialDirection;
            float currentArea = activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange;
            transform.localScale = new Vector3(currentArea, currentArea, currentArea);
        }
        if (IsServer)
            StartCoroutine(Despawn());
    }

    protected override void Update()
    {
        base.Update();
        if (!IsOwner) return;
        if (GameManager.Instance.currentState == GameState.Paused) return;

        transform.Rotate(Vector3.up, 100f * Time.deltaTime);

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
        if (GameManager.Instance.currentState == GameState.Paused) return;

        if (IsServer && IsClientInitialized && other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
        }
    }
}
