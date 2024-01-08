using FishNet.Object;
using UnityEngine;

public class RocketBehaviour : ProjectileBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    [ObserversRpc]
    public void SetProjectileRpc(Vector3 targetEnemyPosition, ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            Vector3 direction = (targetEnemyPosition - transform.position).normalized;
            rb.velocity = activeItem.GetCurrentLevel().speed * Player.Instance.currentProjectileSpeed * direction;
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        }
        if (IsServer)
            StartCoroutine(Despawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsClientInitialized && !despawning && other.transform.root.CompareTag("Enemy"))
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position + Vector3.down, explosionPrefab.transform.rotation);
            Spawn(explosion);
            Collider[] objectsInRange = Physics.OverlapSphere(gameObject.transform.position, activeItem.GetCurrentLevel().area * Player.Instance.currentAttackRange);
            foreach (Collider col in objectsInRange)
            {
                if (col.transform.root.TryGetComponent(out Enemy enemy))
                {
                    enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                }
            }
            despawning = true;
            Despawn(gameObject);
        }
    }
}
