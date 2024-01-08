using FishNet.Object;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    public float CurrentMaxHealth { get; set; }
    public float CurrentMoveSpeed { get; set; }
    public float CurrentAttackRange { get; set; }
    public float CurrentDamage { get; set; }

    private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    [HideInInspector] public float CanBeDamagedByForceField;
    [HideInInspector] public float CanBeDamagedByFire;
    [HideInInspector] public float CanBeDamagedBySawblade;

    [SerializeField] private EnemyStats stats;
    public EnemyStats Stats => stats;

    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private Transform graphics;
    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private Material material;
    private Ragdoll ragdoll;
    [SerializeField] private GameObject healthBar;

    float lastAttackTime = 0;
    public float attackCooldown = 2;
    private float dissolve = -0.1f;
    private bool isStopped = false;
    [SerializeField] private float dissolveSpeed = 0.1f;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();
        agent = GetComponent<NavMeshAgent>();
        CurrentMaxHealth = stats.MaxHealth;
        CurrentMoveSpeed = stats.MoveSpeed;
        CurrentAttackRange = stats.AttackRange;
        CurrentDamage = stats.Damage;
        agent.speed = CurrentMoveSpeed;
        agent.stoppingDistance = CurrentAttackRange;
        currentHealth = CurrentMaxHealth;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (IsServer)
        {
            int rand = Random.Range(0, graphics.childCount);
            RpcRandomZombieLook(rand);
        }
    }

    private void Update()
    {
        if (isDead)
        {
            dissolve += Time.deltaTime * dissolveSpeed;
            material.SetVector("_DissolveOffset", new Vector4(0f, dissolve, 0f, 0f));
            if(IsServer && dissolve >= 0.5f)
            {
                enabled = false;
                GameManager.Instance.EnemyKilled(this);
                Despawn(gameObject);
            }
            return;
        }

        if (!IsServer) return;

        if (GetClosestPlayer() != null)
            player = GetClosestPlayer().transform;

        if (player == null || isStopped) return;

        if (CanBeDamagedByForceField > 0)
            CanBeDamagedByForceField -= Time.deltaTime;

        if (CanBeDamagedByFire > 0)
            CanBeDamagedByFire -= Time.deltaTime;

        if (CanBeDamagedBySawblade > 0)
            CanBeDamagedBySawblade -= Time.deltaTime;

        if (Vector3.Distance(transform.position, player.position) <= CurrentAttackRange)
        {
            agent.isStopped = true;
            Attack();
        }
        else
        {
            animator.SetBool("Attack", false);
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void Attack()
    {
        if(Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;
            player.GetComponent<Player>().TakeDamageServer(CurrentDamage);
        }         
    }

    public void ChangeAgentStatus(bool status)
    {
        agent.enabled = !status;
        isStopped = status;
    }

    GameObject GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float minimumDistance = 1000000f;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < minimumDistance)
            {
                closestPlayer = player;
                minimumDistance = distanceToPlayer;
            }
        }

        return closestPlayer;
    }

    [ObserversRpc(BufferLast = true)]
    void RpcRandomZombieLook(int rand)
    {    
        for (int i = 0; i < graphics.childCount; i++)
        {
            graphics.GetChild(i).gameObject.SetActive(false);
        }

        GameObject mesh = graphics.GetChild(rand).gameObject;
        mesh.SetActive(true);
        material = mesh.GetComponent<SkinnedMeshRenderer>().material;
    }

    [ObserversRpc(RunLocally = true)]
    void RpcSetHealthBar(float currentHealth)
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<Slider>().value = currentHealth / CurrentMaxHealth;
        if (healthBar.GetComponentInChildren<Slider>().value <= 0)
            healthBar.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerTakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        RpcSetHealthBar(currentHealth);
        GameObject blood = Instantiate(bloodPrefab, transform.position + Vector3.up, Quaternion.identity);
        Spawn(blood);

        if (currentHealth <= 0)
        {
            isDead = true;
            agent.enabled = false;
            DropItem();
            RpcDie();
            if (stats.IsBoss)
                GameManager.Instance.ClearedIsland();
        }
    }

    [ObserversRpc(BufferLast = true)]
    void RpcDie()
    {
        isDead = true;
        ragdoll.Die();
        material.SetFloat("_EdgeWidth", 0.3f);
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
    }

    private void DropItem()
    {
        PickableItem item = LootTable.GetItem(stats.Loot);
        GameObject pickupItem = Instantiate(item.prefab, transform.position + Vector3.up, Quaternion.identity);
        Spawn(pickupItem);
    }
}
