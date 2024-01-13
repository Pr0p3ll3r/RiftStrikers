using FishNet.Object;
using UnityEngine;

public class BossXero : Enemy
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private int lives = 10;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        agent.enabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerTakeDamage(float damage)
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
            if (lives > 0) Duplicate();
            else
            {
                foreach (Enemy enemy in GameManager.Instance.enemies)
                {
                    if (!enemy.IsDead && enemy.Stats.IsBoss)
                        return;
                }
                GameManager.Instance.ClearedIsland();
            }
        }
    }

    private void Duplicate()
    {     
        GameObject newBoss1 = Instantiate(bossPrefab, transform.position + Vector3.right * 0.5f, Quaternion.identity);
        GameObject newBoss2 = Instantiate(bossPrefab, transform.position + Vector3.left * 0.5f, Quaternion.identity);
        BossXero boss1 = newBoss1.GetComponent<BossXero>();
        BossXero boss2 = newBoss2.GetComponent<BossXero>();
        boss1.SetStats(CurrentMaxHealth, CurrentDamage, lives - 1);
        boss2.SetStats(CurrentMaxHealth, CurrentDamage, lives - 1);
        Spawn(newBoss1);
        Spawn(newBoss2);
        GameManager.Instance.enemies.Add(boss1);
        GameManager.Instance.enemies.Add(boss2);
    }

    private void SetStats(float health, float damage, int lives)
    {
        this.lives = lives - 1;
        CurrentMaxHealth = health;
        CurrentDamage = damage;
    }
}
