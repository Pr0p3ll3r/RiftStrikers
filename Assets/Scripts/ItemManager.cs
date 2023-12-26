using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private Transform itemsList;
    private List<Item> items = new List<Item>();

    public void AddItem(Item itemData)
    {
        if (itemData is ActiveItem activeItem)
        {
            items.Add(activeItem);
            GameObject itemGO = Instantiate(activeItem.prefab, itemsList);
            itemGO.GetComponent<ActiveItemController>().SetData(activeItem);
            InstanceFinder.ServerManager.Spawn(itemGO);
            Debug.Log("Add active item");
        }
        else
        {
            // passiveItem
        }      
    }
}
