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

    protected float currentHealth;
    protected bool isDead;
    public bool IsDead => isDead;
    [HideInInspector] public float CanBeDamagedByForceField;
    [HideInInspector] public float CanBeDamagedByFire;
    [HideInInspector] public float CanBeDamagedBySawblade;

    [SerializeField] protected EnemyStats stats;
    public EnemyStats Stats => stats;

    [SerializeField] protected GameObject bloodPrefab;
    [SerializeField] private Transform graphics;
    [SerializeField] private GameObject mesh;
    protected Transform player;
    protected Animator animator;
    protected NavMeshAgent agent;
    private Material material;
    private Ragdoll ragdoll;
    [SerializeField] private GameObject healthBar;

    protected float lastAttackTime = 0;
    [SerializeField] protected float attackCooldown = 2;
    [SerializeField] private bool randomLook = false;
    private float dissolve = -0.1f;
    private bool isStopped = false;
    [SerializeField] private float dissolveSpeed = 0.1f;
    [SerializeField] private float dissolveEnd = 0.4f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();
        agent = GetComponent<NavMeshAgent>();
        CurrentMaxHealth = stats.MaxHealth;
        CurrentMoveSpeed = stats.MoveSpeed;
        CurrentAttackRange = stats.AttackRange;
        CurrentDamage = stats.Damage;
        agent.speed = CurrentMoveSpeed;
        currentHealth = CurrentMaxHealth;
        if (!randomLook)
            SetMaterial(mesh);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
        if (IsServer)
        {
            if (!randomLook) return;

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
            if(IsServer && dissolve >= dissolveEnd)
            {
                enabled = false;
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

    protected virtual void Attack()
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
        if(agent)
            agent.enabled = !status;
        isStopped = status;
    }

    private GameObject GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float minimumDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            if (player.GetComponent<Player>().IsDead) continue;

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
    private void RpcRandomZombieLook(int rand)
    {    
        for (int i = 0; i < graphics.childCount; i++)
        {
            graphics.GetChild(i).gameObject.SetActive(false);
        }

        GameObject mesh = graphics.GetChild(rand).gameObject;
        mesh.SetActive(true);
        SetMaterial(mesh);
    }

    private void SetMaterial(GameObject mesh)
    {
        material = mesh.GetComponent<SkinnedMeshRenderer>().material;
        dissolve = material.GetVector("_DissolveOffset").y;
    }

    [ObserversRpc]
    protected void RpcSetHealthBar(float currentHealth)
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<Slider>().value = currentHealth / CurrentMaxHealth;
        if (healthBar.GetComponentInChildren<Slider>().value <= 0)
            healthBar.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerTakeDamage(float damage)
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
            GameManager.Instance.EnemyKilled(this);
            if (stats.IsBoss)
                GameManager.Instance.ClearedIsland();
        }
    }

    [ObserversRpc(BufferLast = true)]
    protected void RpcDie()
    {
        isDead = true;
        ragdoll.Die();
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
    }

    protected void DropItem()
    {
        PickableItem item = LootTable.GetItem(stats.Loot);
        GameObject pickupItem = Instantiate(item.prefab, new Vector3(transform.position.x, 1.5f, transform.position.z), item.prefab.transform.rotation);
        Spawn(pickupItem);
    }
}
