using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance Instance { get; private set; }

    [SyncVar]
    public string nickname;

    [SyncVar]
    public bool isReady;

    [SyncVar]
    public Player controlledPlayer;

    [SerializeField]
    private GameObject playerPrefab;

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

    [ServerRpc]
    public void ServerSetIsReady(bool value)
    {
        isReady = value;
        GameManager.Instance.CheckCanStart();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;
    }

    public void SpawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab);

        Debug.Log("Player Spawn");
        Spawn(player, Owner);

        controlledPlayer = player.GetComponent<Player>();
    }
}
