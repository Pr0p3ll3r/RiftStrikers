using FishNet.Object;
using UnityEngine;

public class CallbackDestroy : NetworkBehaviour
{
    void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    void OnParticleSystemStopped()
    {
        if(IsServer)
        {
            Despawn(gameObject);
        }
    }
}