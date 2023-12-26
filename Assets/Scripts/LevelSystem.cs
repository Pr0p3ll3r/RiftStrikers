﻿using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : NetworkBehaviour
{
    public static LevelSystem Instance { get; private set; }

    private int currentLevel = 1;
    private int currentEXP = 0;

    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Transform itemList;
    [SerializeField] private Item money;
    [SerializeField] private List<Item> availableItems;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expBar;
    private List<Item> ownedItems = new List<Item>();
    private Dictionary<Item, int> votes = new Dictionary<Item, int>();
    private int chosen = 0;
    private int maxPassiveItems = 6;
    private int maxActiveItems = 6;
    private int countPassiveItems = 0;
    private int countActiveItems = 0;
    private int luck = 1;
    private int gainedLevels = 0;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient(); 
        upgradeUI.SetActive(false);
        UpdateLevel(currentLevel, currentEXP, CalculateEXPNeededForNextLevel());
    }

    public void GainExperience(int experience)
    {
        currentEXP += experience;

        int expNeeded = CalculateEXPNeededForNextLevel();
        while (currentEXP >= expNeeded)
        {
            currentEXP -= expNeeded;
            currentLevel++;
            expNeeded = CalculateEXPNeededForNextLevel();
            UpdateLevel(currentLevel, currentEXP, expNeeded);
            gainedLevels++;
        }
        OnNewLevel();
        UpdateLevel(currentLevel, currentEXP, expNeeded);
    }

    [ObserversRpc(BufferLast = true)]
    private void UpdateLevel(int level, int exp, int requireExp)
    {
        levelText.text = $"Level: {level} ({exp}/{requireExp})";
        float percentage = (float)exp / requireExp;
        expBar.value = percentage;
    }

    private int CalculateEXPNeededForNextLevel()
    {
        int expNeeded;

        if (currentLevel >= 1 && currentLevel <= 20)
        {
            expNeeded = 5 + (currentLevel - 1) * 10;
        }
        else if (currentLevel >= 21 && currentLevel <= 40)
        {
            expNeeded = 15 + (currentLevel - 21) * 13;
        }
        else
        {
            expNeeded = 504 + (currentLevel - 41) * 16;
        }

        return expNeeded;
    }

    IEnumerator WaitForItemChoice()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        ShowItems(seed);
        while(chosen != GameManager.Instance.GetLivingPlayers())
        {
            yield return new WaitForSeconds(0.1f);
        }
        (Item item, int index) = ChooseItem();
        HighlightSelectedItem(index);
        yield return new WaitForSeconds(2f);
        if (item.isActive)
            ActiveItemChosenRpc((ActiveItem)item);
        else
            PassiveItemChosenRpc();
    }

    [ObserversRpc]
    private void HighlightSelectedItem(int chosenSkillIndex)
    {
        itemList.GetChild(chosenSkillIndex).GetComponent<Image>().color = Color.green;
    }

    [ObserversRpc]
    private void ActiveItemChosenRpc(ActiveItem chosenItem)
    {
        countActiveItems++;
        if (!ownedItems.Contains(chosenItem))
        {
            chosenItem.AddLevel();
            ownedItems.Add(chosenItem);
            availableItems.Remove(chosenItem);
            Player.Instance.HandleItemSelection(chosenItem);
        }
        else
        {
            Item ownedItem = ownedItems.Find(x => x.itemName == chosenItem.itemName);
            ownedItem.AddLevel();
        }     
        chosen = 0;
        votes.Clear();
        upgradeUI.SetActive(false);
        GameManager.Instance.PauseGame(false);
        OnNewLevel();
    }

    [ObserversRpc]
    private void PassiveItemChosenRpc()
    {
        countPassiveItems++;
        chosen = 0;
        votes.Clear();
        upgradeUI.SetActive(false);
        GameManager.Instance.PauseGame(false);
        OnNewLevel();
    }

    [ObserversRpc]
    private void StartItemChoose()
    {
        foreach (Transform itemSlot in itemList)
        {
            itemSlot.GetComponent<Button>().interactable = true;
            itemSlot.GetComponent<Image>().color = Color.white;
            itemSlot.gameObject.SetActive(false);
        }
    }

    [Server]
    private void OnNewLevel()
    {
        if (gainedLevels <= 0) return;
        gainedLevels--;
        StartItemChoose();
        GameManager.Instance.PauseGame(true);          
        StartCoroutine(WaitForItemChoice());
    }

    [ObserversRpc]
    private void ShowItems(int seed)
    {
        Random.InitState(seed);
        List<Item> tempAvailableItems = new List<Item>(availableItems);
        List<Item> ownedItemsToUpgrade = ownedItems.FindAll(x => x.GetLevel() < x.maxLevel).ToList();

        int i;
        for (i = 0; i < itemList.childCount; i++)
        {
            Transform itemSlot = itemList.GetChild(i);
            itemSlot.GetComponent<Button>().onClick.RemoveAllListeners();

            if (ownedItems.Count < maxPassiveItems + maxActiveItems)
            {             
                if (ownedItemsToUpgrade.Count > 0)
                {
                    if (ChooseItemFromOwnedSkills(ownedItemsToUpgrade, itemSlot))
                        continue;
                }
                if (tempAvailableItems.Count > 0)
                {
                    if (ChooseItemFromAvailable(tempAvailableItems, itemSlot))
                        continue;
                }
            }
            SetMoneyToSlot(itemSlot);
            break;
        }
        ActivateItemSlots(i);
    }

    private bool ChooseItemFromOwnedSkills(List<Item> ownedItemsToUpgrade, Transform itemSlot)
    {
        float ownedChance = 0.5f;
        float randomValue = 0.5f;
        if (randomValue <= ownedChance)
        {
            int random1 = Random.Range(0, ownedItemsToUpgrade.Count);
            //int random2 = Random.Range(0, ownedItemsToUpgrade.Count);

            //if (random1 != random2)
            //{
            //    SetItemSlot(ownedItemsToUpgrade[random1], itemSlot);
            //    ownedItemsToUpgrade.RemoveAt(ownedItemsToUpgrade.IndexOf(ownedItemsToUpgrade[random1]));
            //    return true;
            //}
            SetItemSlot(ownedItemsToUpgrade[random1], itemSlot);
            ownedItemsToUpgrade.RemoveAt(ownedItemsToUpgrade.IndexOf(ownedItemsToUpgrade[random1]));
            return true;
        }
        return false;
    }

    private bool ChooseItemFromAvailable(List<Item> availableItems, Transform itemSlot)
    {
        int countPassiveItems = ownedItems.Count(x => !x.isActive);
        int countActiveItems = ownedItems.Count(x => x.isActive);

        while (availableItems.Count > 0)
        {
            int randomIndex = Random.Range(0, availableItems.Count);
            Item selectedItem = availableItems[randomIndex];

            if ((selectedItem.isActive && countActiveItems < maxActiveItems)
                || (!selectedItem.isActive && countPassiveItems < maxPassiveItems))
            {
                SetItemSlot(selectedItem, itemSlot);
                availableItems.RemoveAt(availableItems.IndexOf(selectedItem));
                return true;
            }

            availableItems.RemoveAt(randomIndex);
        }

        return false;
    }

    private void SetItemSlot(Item item, Transform itemSlot)
    {
        itemSlot.GetComponent<ItemSlot>().SetSlot(item);
        itemSlot.GetComponent<Button>().onClick.AddListener(delegate { VoteForItem(item); });
        votes.Add(item, 0);
    }

    private void SetMoneyToSlot(Transform itemSlot)
    {
        itemSlot.GetComponent<ItemSlot>().SetSlot(money);
        itemSlot.GetComponent<Button>().onClick.AddListener(delegate { VoteForItem(money); });
        votes.Add(money, 0);
    }

    private void VoteForItem(Item skill)
    {
        ServerVote(skill);
        foreach (Transform skillSlot in itemList)
        {
            skillSlot.GetComponent<Button>().interactable = false;
        }
    }

    [ObserversRpc]
    private void ActivateItemSlots(int index)
    {
        for (int i = 0; i <= index; i++)
        {
            itemList.GetChild(i).gameObject.SetActive(true);
        }
        upgradeUI.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerVote(Item skill)
    {
        Item s = votes.First(x => x.Key.itemName == skill.itemName).Key;
        votes[s]++;
        chosen++;
    }

    private (Item, int) ChooseItem()
    {
        Item chosenItem = null;
        int chosenIndex = -1;

        List<Item> maxVotedItems = new List<Item>();
        int maxVotes = 0;

        foreach (KeyValuePair<Item, int> pair in votes)
        {
            if (pair.Value > maxVotes)
            {
                maxVotes = pair.Value;
                maxVotedItems.Clear();
                maxVotedItems.Add(pair.Key);
            }
            else if (pair.Value == maxVotes && pair.Value != 0)
            {
                maxVotedItems.Add(pair.Key);
            }
        }

        if (maxVotedItems.Count > 0)
        {
            int randomIndex = Random.Range(0, maxVotedItems.Count);
            chosenItem = maxVotedItems[randomIndex];

            int index = 0;
            foreach (KeyValuePair<Item, int> pair in votes)
            {
                if (pair.Key.itemName == chosenItem.itemName)
                {
                    chosenIndex = index;
                    break;
                }
                index++;
            }
        }

        return (chosenItem, chosenIndex);
    }
}
