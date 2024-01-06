using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Connection;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private PlayerStats stats;
    public PlayerStats Stats => stats;

    [HideInInspector] public float currentMaxHealth;
    [HideInInspector] public float currentHealthRecovery;
    [HideInInspector] public float currentDamageReduction;
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float currentAttackRange;
    [HideInInspector] public float currentProjectileSpeed;
    [HideInInspector] public float currentAttackDuration;
    [HideInInspector] public float currentAttackCooldown;
    [HideInInspector] public float currentExpGain;
    [HideInInspector] public float currentMoneyGain;
    [HideInInspector] public float currentLootRange;

    private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    private int currentMoney;
    private float healthRecoveryTime = 5f;
    private float currentHealthRecoveryTime;

    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private GameObject itemsContainer;

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
        currentMaxHealth = stats.MaxHealth;
        currentHealthRecovery = stats.HealthRecovery;
        currentDamageReduction = stats.DamageReduction;
        currentMoveSpeed = stats.MoveSpeed;
        currentDamage = stats.Damage;
        currentAttackRange = stats.AttackRange;
        currentProjectileSpeed = stats.ProjectileSpeed;
        currentAttackDuration = stats.AttackDuration;
        currentAttackCooldown = stats.AttackCooldown;
        currentExpGain = stats.ExpGain;
        currentMoneyGain = stats.MoneyGain;
        currentLootRange = stats.LootRange;
        currentHealthRecoveryTime = healthRecoveryTime;
        currentMoney = 0;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (Owner.IsLocalClient)
            Instance = this;
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

        HealthRecovery();
        PullItemsTowardsPlayer();

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TakeDamageServer(20);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            LevelSystem.Instance.GainExperience(5);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServer(float damage)
    {
        if (isDead) return;

        float modifiedDamage = damage * (1 - currentDamageReduction);
        currentHealth -= modifiedDamage;

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
        Destroy(itemsContainer);
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
    }

    private void HealthRecovery()
    {
        if (currentHealthRecovery == 0) return;

        if (currentHealth < currentMaxHealth)
        {
            currentHealthRecoveryTime -= Time.deltaTime;
            if(currentHealthRecoveryTime <= 0) 
            {
                currentHealth += currentMaxHealth * currentHealthRecovery;
                currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
                currentHealthRecoveryTime = healthRecoveryTime;
                hud.RefreshBars(currentHealth);
            }
        }      
    }

    private void PullItemsTowardsPlayer()
    {
        Collider[] itemsInRadius = Physics.OverlapSphere(transform.position, currentLootRange);

        foreach (Collider itemCollider in itemsInRadius)
        {
            if (itemCollider.TryGetComponent<PickupItem>(out var pickupItem) && !itemCollider.CompareTag("HealthPickup"))
            {
                Vector3 directionToPlayer = transform.position - pickupItem.transform.position;
                if (directionToPlayer.magnitude < currentLootRange)
                {
                    pickupItem.transform.position = Vector3.MoveTowards(pickupItem.transform.position, transform.position, 5f * Time.deltaTime);
                }
            }
        }
    }

    public bool HandlePickup(PickableItem item, int value)
    {
        if (item.itemType == ItemType.Health)
        {
            if (currentHealth >= currentMaxHealth) return false;

            currentHealth += value;
        }
        else if (item.itemType == ItemType.Exp)
        {
            value = Mathf.RoundToInt(value * currentExpGain);
            LevelSystem.Instance.GainExperience(value);
        }
        else if (item.itemType == ItemType.Money)
        {
            value = Mathf.RoundToInt(value * currentMoneyGain);
            currentMoney += value;
        }
        return true;
    }

    public void HandleItemSelection(Item item)
    {
        if(item is ActiveItem activeItem)
        {
            itemManager.AddActiveItem(activeItem);
        }
        else if(item is PassiveItem passiveItem)
        {
            if (passiveItem.name == "Money")
            {
                currentMoney += (int)passiveItem.multiplier;
            }
            else
            {
                itemManager.AddPassiveItem(passiveItem);
            }           
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.layer == LayerMask.NameToLayer("Water"))
    //    {
    //        DieServer();
    //    }
    //}
}