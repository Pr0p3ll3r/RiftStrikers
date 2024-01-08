using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class KnifeBehaviour : ProjectileBehaviour
{
    private int currentPierce;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    [ObserversRpc]
    public void SetProjectileRpc(Vector3 targetEnemyPosition, ActiveItem activeItem)
    {
        this.activeItem = activeItem;
        currentPierce = activeItem.GetCurrentLevel().pierce;
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
        if (IsServer && IsClientInitialized && !despawning && other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            if (!hitEnemies.Contains(enemy))
            {
                enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
                hitEnemies.Add(enemy);
                currentPierce--;
            }

            if (currentPierce <= 0)
            {
                despawning = true;
                Despawn(gameObject);
            }
        }
    }
}
