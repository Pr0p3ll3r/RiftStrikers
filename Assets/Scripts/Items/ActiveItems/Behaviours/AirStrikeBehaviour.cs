using FishNet.Object;
using UnityEngine;

public class AirStrikeBehaviour : NetworkBehaviour
{
    private ActiveItem activeItem;

    void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void SetProjectile(ActiveItem item)
    {
        activeItem = item;
    }

    void OnParticleSystemStopped()
    {
        if (IsServer)
        {
            Despawn(gameObject);
        }
    }
}
