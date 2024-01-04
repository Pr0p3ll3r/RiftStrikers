using System.Collections.Generic;
using UnityEngine;

public class Knife : ActiveItemController
{
    [SerializeField] private GameObject knifePrefab;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        List<Enemy> tempList = new List<Enemy>(GameManager.Instance.Enemies);
        for (int i = 0; i < activeItem.GetCurrentLevel().projectiles; i++)
        {
            GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(transform.position, activeItem.GetCurrentLevel().range * Player.Instance.currentAttackRange, tempList);
            if (closestEnemy != null)
            {
                GameObject spawnedKnife = Instantiate(knifePrefab, transform.position, knifePrefab.transform.rotation);
                Spawn(spawnedKnife);
                spawnedKnife.GetComponent<KnifeBehaviour>().SetProjectile(closestEnemy, activeItem);
                if (closestEnemy.TryGetComponent<Enemy>(out var enemyComponent))
                {
                    tempList.Remove(enemyComponent);
                }
            }
        }      
    }
}
