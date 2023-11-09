using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Connection;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    private bool isDead;
    public bool IsDead => isDead;

    [SyncVar]
    public int currentHealth;

    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private AudioSource deathSound;

    private PlayerController controller;
    private PlayerHUD hud;
    private WeaponManager wm;
    private Ragdoll ragdoll;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {       
        hud = GetComponent<PlayerHUD>();
        wm = GetComponent<WeaponManager>();
        controller = GetComponent<PlayerController>();
        ragdoll = GetComponent<Ragdoll>();
        hud.RefreshBars(currentHealth);     
    }

    void Update()
    {
        if (!IsOwner) return;

#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TakeDamageServer(20);
        }
#endif
    }
 
    [ServerRpc]
    public void TakeDamageServer(int damage)
    {
        if (isDead) return;
    
        currentHealth -= damage;
        hurtSound.Play();
        TakeDamageRpc(currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("SERVER: Player died");
            DieServer();
        }
    }

    [ObserversRpc]
    public void TakeDamageRpc(int newHealth)
    {
        hud.ShowVignette();
        hud.RefreshBars(newHealth);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DieServer()
    {
        DieRpc();
    }

    [ObserversRpc]
    private void DieRpc()
    {
        ragdoll.Die();
        deathSound.Play();
        isDead = true;
        controller.OnDeath();
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
    }
}