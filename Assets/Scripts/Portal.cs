using FishNet.Object;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            ServerPlayerInPortal(other.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerPlayerInPortal(GameObject other)
    {
        Despawn(other);
        MapGenerator.Instance.PlayersInPortal();
    }
}
