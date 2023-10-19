using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour, IDamageable
{
    [SyncVar]
    public int currentHealth = 100;

    public int damage = 5;
    public float attackDistance = 5f;
    public float speed = 5f;
    [SyncVar]
    public bool isDead;
    public int maxHealth = 100;

    public int exp;
    public int money;

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
    [SerializeField] private float dissolveSpeed = 0.1f;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("StarClient");
        RandomZombieLookServer();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        ragdoll = GetComponent<Ragdoll>();
        if (GetClosestPlayer() != null)
            player = GetClosestPlayer().transform;
    }

    private void Update()
    {
        if (isDead)
        {
            dissolve += Time.deltaTime * dissolveSpeed;
            material.SetVector("_DissolveOffset", new Vector4(0f, dissolve, 0f, 0f));
            if(IsServer && dissolve >= 0.5f)
            {
                Despawn(gameObject);
            }
            return;
        }

        if (!IsServer) return;

        if (GetClosestPlayer() != null)
            player = GetClosestPlayer().transform;

        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            agent.isStopped = true;
            Attack();
        }
        else
        {
            Debug.Log("Update");
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
            player.GetComponent<Player>().TakeDamageServer(damage);
        }         
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

    [ServerRpc(RequireOwnership = false)]
    void RandomZombieLookServer()
    {
        int rand = Random.Range(0, graphics.childCount);
        RandomZombieLookRpc(rand);
    }

    [ObserversRpc]
    void RandomZombieLookRpc(int rand)
    {    
        for (int i = 0; i < graphics.childCount; i++)
        {
            graphics.GetChild(i).gameObject.SetActive(false);
        }

        GameObject mesh = graphics.GetChild(rand).gameObject;
        mesh.SetActive(true);
        material = mesh.GetComponent<SkinnedMeshRenderer>().material;
    }

    void SetHealthBar()
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<Slider>().value = (float)currentHealth / maxHealth;
        if (healthBar.GetComponentInChildren<Slider>().value <= 0)
            healthBar.SetActive(false);
    }

    [ObserversRpc]
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        SetHealthBar();
        if (IsServer && currentHealth <= 0)
        {
            DieServer();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DieServer()
    {
        DieRpc();
    }

    [ObserversRpc]
    void DieRpc()
    {
        isDead = true;
        agent.isStopped = true;
        ragdoll.Die();
        SetHealthBar();
        material.SetFloat("_EdgeWidth", 0.3f);
        gameObject.layer = LayerMask.NameToLayer("NotCollide");
        foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = LayerMask.NameToLayer("NotCollide");
        }
    }
}
