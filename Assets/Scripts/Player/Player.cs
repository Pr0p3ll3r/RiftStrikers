using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    public bool isDead;

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
#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TakeDamage(20);
        }
#endif
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        hurtSound.Play();
        hud.ShowVignette();
        currentHealth -= damage;
        hud.RefreshBars(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ragdoll.Die();
        deathSound.Play();
        isDead = true;
        controller.Control(false);
        wm.ShowWeapon();
    }
}