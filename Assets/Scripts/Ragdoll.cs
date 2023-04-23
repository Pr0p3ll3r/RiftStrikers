using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    private Animator animator;
    private Collider mainCollider;
    private Rigidbody[] rigidbodies;
    private Collider[] colliders;

    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        SetRigidbodyState(true);
        SetColliderState(false);
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
    }

    public void Die()
    {
        animator.enabled = false;
        SetRigidbodyState(false);
        SetColliderState(true);
    }

    void SetRigidbodyState(bool state)
    {    
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = state;
        }
    }

    void SetColliderState(bool state)
    {   
        for (int i = 1; i < colliders.Length; i++)
        {
            colliders[i].isTrigger = !state;
        }

        if (mainCollider) mainCollider.enabled = !state;
    }
}
