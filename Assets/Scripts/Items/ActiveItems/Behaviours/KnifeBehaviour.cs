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
        rb.velocity = direction * activeItem.GetCurrentLevel().speed * Player.Instance.CurrentProjectileSpeed;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.Rotate(Vector3.up, -90f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.CurrentDamage);
            Despawn(gameObject);
        }
    }
}
