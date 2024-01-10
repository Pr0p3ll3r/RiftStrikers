using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [SerializeField] private Transform itemsList;
    private List<ActiveItemController> activeItems = new List<ActiveItemController>();

    [ServerRpc(RequireOwnership = false)]
    public void AddActiveItem(ActiveItem item)
    {
        GameObject itemGO = Instantiate(item.prefab, itemsList);
        Spawn(itemGO, Owner);
        itemGO.transform.SetParent(itemsList);
        itemGO.transform.localPosition = Vector3.zero;
        ActiveItemController controller = itemGO.GetComponent<ActiveItemController>();
        controller.SetData(item, transform);
        AddActiveItemRpc(controller);
    }

    [ObserversRpc]
    private void AddActiveItemRpc(ActiveItemController controller)
    {
        activeItems.Add(controller);
    }

    public void LevelUpActiveItem(ActiveItem item)
    {
        ActiveItemController ownedItem = activeItems.Find(x => x.activeItem.itemName == item.itemName);
        ownedItem.AddLevel();
    }

    public void AddPassiveItem(PassiveItem item)
    {
        switch (item.itemType)
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
