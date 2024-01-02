using FishNet.Object;
using UnityEngine;

public class KnifeBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Rigidbody rb;

    public void SetProjectile(GameObject enemy, ActiveItem item)
    {
        activeItem = item;
        rb = GetComponent<Rigidbody>();
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        rb.velocity = activeItem.GetCurrentLevel().Speed * Player.Instance.currentProjectileSpeed * direction;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.Rotate(Vector3.up, -90f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().Damage * Player.Instance.currentDamage);
            Despawn(gameObject);
        }
    }
}
