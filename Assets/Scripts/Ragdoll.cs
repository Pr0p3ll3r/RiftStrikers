using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    void Start()
    { 
        SetRigidbodyState(true);
        SetColliderState(false);
    }

    public void Die()
    {
        GetComponent<Animator>().enabled = false;
        SetRigidbodyState(false);
        SetColliderState(true);
    }

    void SetRigidbodyState(bool state)
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = state;
        }
    }

    void SetColliderState(bool state)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 1; i < colliders.Length; i++)
        {
            colliders[i].isTrigger = !state;
        }

        //GetComponent<Collider>().enabled = !state;
    }
}
