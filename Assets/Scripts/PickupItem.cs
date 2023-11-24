using FishNet.Object;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    [SerializeField] private PickableItem item;
    [SerializeField] private int value;

    public void SetItem(PickableItem item, int value)
    {
        this.item = item;
        this.value = value;
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
        player.GetComponent<Player>().HandlePickup(item, value);
        Despawn(gameObject);
    }
}
