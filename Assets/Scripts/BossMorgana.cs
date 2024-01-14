using UnityEngine;

public class BossMorgana : Enemy
{
    protected override void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;
            player.GetComponent<Player>().TakeDamageServer(CurrentDamage);
            currentHealth += player.GetComponent<Player>().GetHealAmount(CurrentDamage);
            currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
            RpcSetHealthBar(currentHealth);
        }
    }
}
