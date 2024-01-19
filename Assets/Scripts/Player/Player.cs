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
    private float healthRecoveryTime = 1f;

    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private GameObject itemsContainer;

    private PlayerController controller;
    private PlayerHUD hud;
    private Ragdoll ragdoll;
    private ItemManager itemManager;
    public bool CanControl { get; set; } = true;
    public bool AutoAim { get; set; }

    private void Awake()
    {
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

        if(CloudData.PlayerData.MaxHealth > 0)
            currentMaxHealth *= (1 + CloudData.PlayerData.MaxHealth * 10 / 100f);
        currentHealth = currentMaxHealth;

        if (CloudData.PlayerData.HealthRecovery > 0)
            currentHealthRecovery += (CloudData.PlayerData.HealthRecovery * 0.1f);

        if (CloudData.PlayerData.DamageReduction > 0)
            currentDamageReduction += CloudData.PlayerData.DamageReduction;

        if (CloudData.PlayerData.MoveSpeed > 0)
            currentMoveSpeed *= (1 + CloudData.PlayerData.MoveSpeed * 5 / 100f);

        if (CloudData.PlayerData.Damage > 0)
            currentDamage += (CloudData.PlayerData.Damage * 5 / 100f);

        if (CloudData.PlayerData.AttackDuration > 0)
            currentAttackDuration += (CloudData.PlayerData.AttackDuration * 10 / 100f);

        if (CloudData.PlayerData.AttackRange > 0)
            currentAttackRange += (CloudData.PlayerData.AttackRange * 2.5f / 100f);

        if (CloudData.PlayerData.ProjectileSpeed > 0)
            currentProjectileSpeed += (CloudData.PlayerData.ProjectileSpeed * 10 / 100f);

        if (CloudData.PlayerData.AttackDuration > 0)
            currentAttackDuration += (CloudData.PlayerData.AttackDuration * 15 / 100f);

        if (CloudData.PlayerData.LootRange > 0)
            currentLootRange += (CloudData.PlayerData.LootRange * 25 / 100f);

        if (CloudData.PlayerData.ExpGain > 0)
            currentExpGain += (CloudData.PlayerData.ExpGain * 3 / 100f);

        if (CloudData.PlayerData.MoneyGain > 0)
            currentMoneyGain += (CloudData.PlayerData.MoneyGain * 10 / 100f);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (base.Owner.IsLocalClient)
            Instance = this;
    }

    void Start()
    {       
        hud = GetComponent<PlayerHUD>();
        controller = GetComponent<PlayerController>();
        ragdoll = GetComponent<Ragdoll>();
        itemManager = GetComponent<ItemManager>();
        hud.RefreshBars(currentHealth);
        AutoAim = PlayerPrefs.GetInt("AutoAim", 1) == 1;
    }

    void Update()
    {
        if (!IsOwner || GameManager.Instance.currentState == GameState.Paused) return;

        HealthRecovery();
        PullItemsTowardsPlayer();

        //if (Keyboard.current.tKey.wasPressedThisFrame)
        //{
        //    TakeDamageServer(20);
        //}
        //if (Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    LevelSystem.Instance.GainExperience(5);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServer(float damage)
    {
        if (isDead) return;

        float modifiedDamage = damage + currentDamageReduction;
        currentHealth -= Mathf.Max(0, modifiedDamage);

        hurtSound.Play();
        RefreshHudRpc(Owner, currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("SERVER: Player died");
            DieServer();
        }
    }

    public float GetHealAmount(float damage)
    {
        float modifiedDamage = damage + currentDamageReduction;
        modifiedDamage = Mathf.Max(0, modifiedDamage);
        return modifiedDamage;
    }

    [TargetRpc]
    public void RefreshHudRpc(NetworkConnection conn, float newHealth)
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
        hud.StopAllCoroutines();
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
        if(GameManager.Instance.GetLivingPlayers() > 0)
        {
            PlayerInstance player = GameManager.Instance.players.Find(x => !x.controlledPlayer.isDead);
            if(player != null)
            {
                Camera.main.GetComponent<CameraFollow>().SetPlayer(player.controlledPlayer.transform);
            }
        }
        if(IsServer)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void HealthRecovery()
    {
        if (currentHealthRecovery == 0) return;

        if (currentHealth < currentMaxHealth)
        {
            healthRecoveryTime -= Time.deltaTime;
            if(healthRecoveryTime <= 0) 
            {
                currentHealth += currentHealthRecovery;
                currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
                healthRecoveryTime = 1f;
                hud.RefreshBars(currentHealth);
            }
        }      
    }

    [ServerRpc(RequireOwnership = false)]
    private void PullItemsTowardsPlayer()
    {
        Collider[] itemsInRadius = Physics.OverlapSphere(transform.position, currentLootRange);

        foreach (Collider itemCollider in itemsInRadius)
        {
            if (itemCollider.transform.root.CompareTag("Pickup"))
            {
                PickupItem pickupItem = itemCollider.transform.root.GetComponent<PickupItem>();
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
            hud.RefreshBars(currentHealth);
        }
        else if (item.itemType == ItemType.Exp)
        {
            value = Mathf.RoundToInt(value * currentExpGain);
            LevelSystem.Instance.GainExperience(value);
        }
        else if (item.itemType == ItemType.Money)
        {
            value = Mathf.RoundToInt(value * currentMoneyGain);
            GameManager.Instance.AddMoneyRpc(value);
        }
        return true;
    }

    public void HandleItemSelection(Item item)
    {
        if(item is ActiveItem activeItem)
        {
            if (activeItem.level > 0)
            {
                itemManager.LevelUpActiveItem(activeItem);
            }  
            else
            {
                itemManager.AddActiveItem(activeItem);
                hud.AddItemUI(activeItem);
            }            
        }
        else if(item is PassiveItem passiveItem)
        {
            if (passiveItem.itemName == "Money")
            {
                if (IsServer)
                    GameManager.Instance.AddMoneyRpc((int)passiveItem.multiplier);
            }
            else if (passiveItem.level > 0)
            {             
                itemManager.AddPassiveItem(passiveItem);
            }
            else
            {
                itemManager.AddPassiveItem(passiveItem);
                hud.AddItemUI(passiveItem);
            }
        }
    }

    public void ReaddItems(Item item)
    {
        if (item is ActiveItem activeItem)
        {
            itemManager.AddActiveItem(activeItem);
        }
        else if (item is PassiveItem passiveItem)
        {
            itemManager.AddPassiveItem(passiveItem);
        }
    }

    public void DisableVignette()
    {
        hud.DisableVignette();
    }
}