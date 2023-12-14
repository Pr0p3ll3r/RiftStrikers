using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance Instance { get; private set; }

    [SyncVar] public Player controlledPlayer;

    [SerializeField] private GameObject playerPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameManager.Instance.players.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        GameManager.Instance.players.Remove(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;
    }

    [ObserversRpc(BufferLast = true)]
    public void SpawnPlayer(bool canControl = true)
    {
        GameObject spawnPoint = MapGenerator.Instance.GetRandomEmptyLand();
        GameObject player = Instantiate(playerPrefab, spawnPoint.transform.position + Vector3.up, Quaternion.identity);

        Debug.Log("Player Spawn");
        Spawn(player, Owner);

        controlledPlayer = player.GetComponent<Player>();
        controlledPlayer.CanControl = canControl;
    }
}
