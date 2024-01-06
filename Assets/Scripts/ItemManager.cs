using FishNet.Object;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [SerializeField] private Transform itemsList;

    public void AddActiveItem(ActiveItem item)
    {
        GameObject itemGO = Instantiate(item.prefab, itemsList);
        itemGO.GetComponent<ActiveItemController>().SetData(item);
        Spawn(itemGO, Owner);
    }

    public void AddPassiveItem(PassiveItem item)
    {
        switch (item.ItemType)
        {
            case PassiveItemType.MaxHealth:
                Player.Instance.currentMaxHealth *= 1 + item.multiplier / 100f;
                break;
            case PassiveItemType.HealthRecovery:
                Player.Instance.currentHealthRecovery += item.multiplier / 100f;
                break;
            case PassiveItemType.DamageReduction:
                Player.Instance.currentDamageReduction += item.multiplier / 100f;
                break;
            case PassiveItemType.MoveSpeed:
                Player.Instance.currentMoveSpeed *= 1 + item.multiplier / 100f;
                break;
            case PassiveItemType.Damage:
                Player.Instance.currentDamage += item.multiplier / 100f;
                break;
            case PassiveItemType.AttackRange:
                Player.Instance.currentAttackRange += item.multiplier / 100f;
                break;
            case PassiveItemType.ProjectileSpeed:
                Player.Instance.currentProjectileSpeed += item.multiplier / 100f;
                break;
            case PassiveItemType.AttackDuration:
                Player.Instance.currentAttackDuration += item.multiplier / 100f;
                break;
            case PassiveItemType.AttackCooldown:
                Player.Instance.currentAttackCooldown += item.multiplier / 100f;
                break;
            case PassiveItemType.ExpGain:
                Player.Instance.currentExpGain += item.multiplier / 100f;
                break;
            case PassiveItemType.MoneyGain:
                Player.Instance.currentMoneyGain += item.multiplier / 100f;
                break;
            case PassiveItemType.LootRange:
                Player.Instance.currentLootRange += item.multiplier / 100f;
                break;
        }
    }
}
