using UnityEngine;

public class SawbladeController : ActiveItemController
{
    [SerializeField] private GameObject sawbladePrefab;

    protected override void Start()
    {
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown) + (activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
    }

    protected override void Attack()
    {
        currentCooldown = (activeItem.GetCurrentLevel().cooldown * Player.Instance.currentAttackCooldown) + (activeItem.GetCurrentLevel().duration * Player.Instance.currentAttackDuration);
        int amount = activeItem.GetCurrentLevel().projectiles;
        float angleStep = 360f / amount;
        for (int i = 0; i < amount; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPosition = transform.position + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * activeItem.GetCurrentLevel().range);

            GameObject sawblade = Instantiate(sawbladePrefab, spawnPosition, Quaternion.identity);
            sawblade.GetComponent<SawbladeBehaviour>().SetProjectile(activeItem, Player.Instance.transform);
            Spawn(sawblade);
        }
    }
}
