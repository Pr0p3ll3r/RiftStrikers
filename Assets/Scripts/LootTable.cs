using System.Linq;
using UnityEngine;

public static class LootTable
{
    public static PickableItem GetItem(PickableItem[] loot)
    {
        int totalWeight = loot.Sum(item => item.weight);
        int roll = Random.Range(0, totalWeight);

        for (int i = 0; i < loot.Length; i++)
        {
            roll -= loot[i].weight;
            if (roll < 0)
            {
                return loot[i];
            }
        }
        return loot[0];
    }
}
