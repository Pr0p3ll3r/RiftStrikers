using FishNet.Object;
using System.Collections;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    protected ActiveItem activeItem;
    protected Vector3 lastVelocity;
    protected bool despawning = false;
    private bool isPaused = false;
    protected Rigidbody rb;

    protected virtual void Update()
    {
        if (!IsOwner) return;

        if (GameManager.Instance.currentState == GameState.Paused)
            ShouldMove(false);
        else
            ShouldMove(true);
    }

    private void ShouldMove(bool shouldMove)
    {
        if (shouldMove && isPaused)
        {
            rb.velocity = lastVelocity;
            isPaused = false;
        }
        else if (!shouldMove && !isPaused)
        {
            lastVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
            isPaused = true;
        }
    }

    protected IEnumerator Despawn()
    {
        yield return new WaitForSeconds(activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        despawning = true;
        Despawn(gameObject);
    }
}
