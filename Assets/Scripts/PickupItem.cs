using FishNet.Object;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    [SerializeField] private PickableItem item;
    private float timer;

    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsClientInitialized && timer <= 0 && other.transform.root.TryGetComponent(out Player player))
        {
            timer = 0.1f;
            ServerPickup(player);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerPickup(Player player)
    {
        if(player.HandlePickup(item, item.value))
            Despawn(gameObject);
    }
}
