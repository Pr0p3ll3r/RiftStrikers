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

        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsClientInitialized && timer <= 0 && other.transform.root.TryGetComponent(out Player player))
        {
            timer = 0.2f;
            ServerPickup(player);
        }
    }

    private void ServerPickup(Player player)
    {
        if(player.HandlePickup(item, item.value))
        {
            Disable();
            StartCoroutine(Wait());
        }        
    }

    [ObserversRpc]
    private void Disable()
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
