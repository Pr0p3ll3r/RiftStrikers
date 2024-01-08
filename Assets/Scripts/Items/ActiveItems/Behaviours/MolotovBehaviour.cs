using FishNet.Object;
using UnityEngine;

public class MolotovBehaviour : ProjectileBehaviour
{
    [SerializeField] private GameObject firePrefab;

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

    protected override void Update()
    {
        base.Update();
        if (!IsOwner) return;
        if (GameManager.Instance.currentState == GameState.Paused) return;

        transform.Rotate(Vector3.right, 100f * Time.deltaTime);      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsClientInitialized && !despawning && other.transform.root.CompareTag("Enemy"))
        {
            GameObject fire = Instantiate(firePrefab, transform.position + Vector3.down, firePrefab.transform.rotation);
            Spawn(fire);
            fire.GetComponent<FireBehaviour>().SetProjectileRpc(activeItem);
            despawning = true;
            Despawn(gameObject);
        }
    }
}
