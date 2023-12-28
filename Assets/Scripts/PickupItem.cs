using FishNet.Object;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    [SerializeField] private PickableItem item;

    public void SetItem(PickableItem item)
    {
        this.item = item;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            ServerPickup(other.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerPickup(GameObject player)
    {
        if(player.GetComponent<Player>().HandlePickup(item, item.value))
            Despawn(gameObject);
    }
}
