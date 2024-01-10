using FishNet.Object;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled && IsServer && other.CompareTag("Player"))
            PlayerInPortal(other.gameObject);
    }

    private void PlayerInPortal(GameObject other)
    {
        Despawn(other);
        GameManager.Instance.PlayersInPortal();
    }
}
