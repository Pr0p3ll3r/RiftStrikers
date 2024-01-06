using FishNet.Object;
using System;
using System.Collections;
using UnityEngine;

public class SawbladeBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;
    private Transform playerTransform;
    private GameObject parent;

    private void Start()
    {
        StartCoroutine(Despawn());
        CreateSawbladeParent();
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration);
        Despawn(gameObject);
        Destroy(parent);
    }

    private void Update()
    {
        RotateSawblade();
    }

    private void CreateSawbladeParent()
    {
        GameObject parent = new GameObject("SawbladeParent");
        this.parent = parent;
        parent.transform.position = transform.position;
        transform.SetParent(parent.transform);
    }

    private void RotateSawblade()
    {
        parent.transform.Rotate(Vector3.up, activeItem.GetCurrentLevel().speed * Time.deltaTime);

        Vector3 desiredPosition = playerTransform.position + (Quaternion.Euler(0f, parent.transform.eulerAngles.y, 0f) * Vector3.forward * activeItem.GetCurrentLevel().range);
        transform.position = desiredPosition;
    }

    public void SetProjectile(ActiveItem item, Transform player)
    {
        activeItem = item;
        playerTransform = player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.transform.root.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.ServerTakeDamage(activeItem.GetCurrentLevel().damage * Player.Instance.currentDamage);
        }
    }
}
