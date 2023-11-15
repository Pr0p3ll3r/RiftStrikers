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
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Player>().HandlePickup(item, value);
            ServerDestroy();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDestroy()
    {
        if (IsServer)
        { 
            Despawn(gameObject);
        }
    }
}
