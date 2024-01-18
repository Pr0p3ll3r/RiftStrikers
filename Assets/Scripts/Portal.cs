using FishNet.Object;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    public bool canBeUsed = false;
    private float timer;

    private void Update()
    {
        if (!IsServer) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !canBeUsed) return;

        if (timer <= 0 && other.transform.root.CompareTag("Player"))
        {
            timer = 0.2f;
            PlayerInPortal(other.transform.root.gameObject);
        }        
    }

    private void PlayerInPortal(GameObject player)
    {
        RpcDisable(player);
        GameManager.Instance.PlayersInPortal();
    }

    [ObserversRpc]
    private void RpcDisable(GameObject player)
    {
        player.SetActive(false);
    }
}
