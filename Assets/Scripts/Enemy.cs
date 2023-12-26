using FishNet.Object;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;

    [SerializeField] private EnemyStats stats;
    public EnemyStats Stats => stats;

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
        agent.speed = stats.moveSpeed;
        currentHealth = stats.maxHealth;
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

        if (Vector3.Distance(transform.position, player.position) <= stats.attackRange)
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
            player.GetComponent<Player>().TakeDamageServer(stats.damage);
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
        healthBar.GetComponentInChildren<Slider>().value = currentHealth / stats.maxHealth;
        if (healthBar.GetComponentInChildren<Slider>().value <= 0)
            healthBar.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerTakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        RpcSetHealthBar(currentHealth);
        if (currentHealth <= 0)
        {
            isDead = true;
            agent.enabled = false;
            DropLoot();
            RpcDie();
            if (stats.isBoss)
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

    private void DropLoot()
    {
        foreach (PickableItem item in stats.loot)
        {
            int randomChance = Random.Range(0, 100);
            if (randomChance <= item.dropChance)
            {
                GameObject pickupItem = Instantiate(item.prefab, transform.position + Vector3.up, Quaternion.identity);
                Spawn(pickupItem);
                switch (item.itemType)
                {
                    case ItemType.Exp:
                        pickupItem.GetComponent<PickupItem>().SetItem(item, stats.exp);
                        break;
                    case ItemType.Money:
                        pickupItem.GetComponent<PickupItem>().SetItem(item, stats.money);
                        break;
                }
            }
        }
    }
}
