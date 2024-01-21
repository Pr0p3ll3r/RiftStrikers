using FishNet.Object;
using System.Collections;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    [SerializeField] private PickableItem item;
    [SerializeField] private GameObject mesh;
    private AudioSource pickupSound;
    private float timer;

    private void Start()
    {
        pickupSound = GetComponent<AudioSource>();
    }

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
        if (!IsServer) return;

        if (IsServerInitialized && timer <= 0 && other.TryGetComponent(out Player player))
        {
            timer = 0.2f;
            Pickup(player);
        }
    }

    private void Pickup(Player player)
    {
        if(player.HandlePickup(item, item.value))
        {
            RpcDisable();
            StartCoroutine(Wait());
        }        
    }

    [ObserversRpc]
    private void RpcDisable()
    {
        pickupSound.Play();
        mesh.SetActive(false);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        Despawn(gameObject);
    }
}
