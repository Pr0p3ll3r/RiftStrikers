using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Connection;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private PlayerStats stats;
    public PlayerStats Stats => stats;

    public float CurrentMaxHealth { get; set; }
    public float CurrentHealthRecovery { get; set; }
    public float CurrentArmor { get; set; }
    public float CurrentMoveSpeed { get; set; }
    public float CurrentDamage { get; set; }
    public float CurrentAttackRange { get; set; }
    public float CurrentProjectileSpeed { get; set; }
    public float CurrentAttackDuration { get; set; } 
    public float CurrentAttackCooldown { get; set; }
    public float CurrentExpGain { get; set; }
    public float CurrentMoneyGain { get; set; }
    public float CurrentLootRange { get; set; }

    private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    private int currentMoney;
    private float healthRecoveryTime = 5f;
    private float currentHealthRecoveryTime;

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
        CurrentMaxHealth = stats.MaxHealth;
        CurrentHealthRecovery = stats.HealthRecovery;
        CurrentArmor = stats.Armor;
        CurrentMoveSpeed = stats.MoveSpeed;
        CurrentDamage = stats.Damage;
        CurrentAttackRange = stats.AttackRange;
        CurrentProjectileSpeed = stats.ProjectileSpeed;
        CurrentAttackDuration = stats.AttackDuration;
        CurrentAttackCooldown = stats.AttackCooldown;
        CurrentExpGain = stats.ExpGain;
        CurrentMoneyGain = stats.MoneyGain;
        CurrentLootRange = stats.LootRange;
        currentMoney = 0;
        currentHealthRecoveryTime = healthRecoveryTime;
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

        HealthRecovery();
        PullItemsTowardsPlayer();

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

        float reducedDamage = Mathf.Max(0, damage - (damage * (CurrentArmor / 100f)));
        currentHealth -= reducedDamage;

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

    private void HealthRecovery()
    {
        if (currentHealth < CurrentMaxHealth)
        {
            currentHealthRecoveryTime -= Time.deltaTime;
            if(currentHealthRecoveryTime <= 0) 
            {
                currentHealth += CurrentMaxHealth * (CurrentHealthRecovery / 100f);
                currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
                currentHealthRecoveryTime = healthRecoveryTime;
                hud.RefreshBars(currentHealth);
            }
        }
    }

    private void PullItemsTowardsPlayer()
    {
        Collider[] itemsInRadius = Physics.OverlapSphere(transform.position, CurrentLootRange);

        foreach (Collider itemCollider in itemsInRadius)
        {
            if (itemCollider.TryGetComponent<PickupItem>(out var pickupItem) && !itemCollider.CompareTag("HealthPickup"))
            {
                Vector3 directionToPlayer = transform.position - pickupItem.transform.position;
                if (directionToPlayer.magnitude < CurrentLootRange)
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
            if (currentHealth >= CurrentMaxHealth) return false;

            currentHealth += value;
        }
        else if (item.itemType == ItemType.Exp)
        {
            LevelSystem.Instance.GainExperience(value);
        }
        else if (item.itemType == ItemType.Money)
        {
            currentMoney += Mathf.RoundToInt(value * CurrentMoneyGain);
        }
        return true;
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