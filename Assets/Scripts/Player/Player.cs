using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Connection;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private PlayerStats stats;
    public PlayerStats Stats => stats;

    private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;    

    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private AudioSource deathSound;

    private PlayerController controller;
    private WeaponManager weaponManager;
    private PlayerHUD hud;
    private Ragdoll ragdoll;
    private ItemManager itemManager;
    public bool CanControl { get; set; } = true;
    public bool AutoAim { get; set; }

    private void Awake()
    {
        currentHealth = stats.MaxHealth;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(IsOwner) Instance = this;
        weaponManager.Equip(0);
    }

    void Start()
    {       
        hud = GetComponent<PlayerHUD>();
        controller = GetComponent<PlayerController>();
        ragdoll = GetComponent<Ragdoll>();
        itemManager = GetComponent<ItemManager>();
        weaponManager = GetComponent<WeaponManager>();
        hud.RefreshBars(currentHealth);
        AutoAim = PlayerPrefs.GetInt("AutoAim", 1) == 1;
    }

    void Update()
    {
        if (!IsOwner || GameManager.Instance.currentState == GameState.Paused) return;

#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TakeDamageServer(20);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            LevelSystem.Instance.GainExperience(20);
        }
#endif
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServer(float damage)
    {
        if (isDead) return;
    
        currentHealth -= damage;
        hurtSound.Play();
        TakeDamageRpc(Owner, currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("SERVER: Player died");
            DieServer();
        }
    }

    [TargetRpc]
    public void TakeDamageRpc(NetworkConnection conn, float newHealth)
    {
        hud.ShowVignette();
        hud.RefreshBars(newHealth);
    }

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

    public void HandlePickup(PickableItem item, int value)
    {
        if (item.itemType == ItemType.Health)
        {
            // Obsługa zdrowia
        }
        else if (item.itemType == ItemType.Exp)
        {
            LevelSystem.Instance.GainExperience(value);
        }
        else if (item.itemType == ItemType.Money)
        {
            // Obsługa pieniędzy
        }
    }

    public void HandleItemSelection(Item item)
    {
        itemManager.AddItem(item);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.layer == LayerMask.NameToLayer("Water"))
    //    {
    //        DieServer();
    //    }
    //}
}