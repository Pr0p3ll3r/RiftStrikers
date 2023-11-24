using FishNet;
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
    [SerializeField] private Transform skillList;
    [SerializeField] private Skill money;
    [SerializeField] private List<Skill> availableSkills;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expBar;
    private List<Skill> ownedSkills = new List<Skill>();
    private Dictionary<Skill, int> votes = new Dictionary<Skill, int>();
    private int chosen = 0;
    private int maxPassiveSkills = 6;
    private int maxActiveSkills = 6;

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
            OnNewLevel();
        }
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

    IEnumerator WaitForSkillChoice()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        ShowSkills(seed);
        while(chosen != GameManager.Instance.GetLivingPlayers())
        {
            yield return new WaitForSeconds(0.1f);
        }
        (Skill skill, int index) = ChooseSkill();
        HighlightSelectedSkill(index);
        yield return new WaitForSeconds(2f);
        SkillChosenRpc(skill);
        GameManager.Instance.ChangeEnemiesStatus(true);
    }

    [ObserversRpc]
    private void HighlightSelectedSkill(int chosenSkillIndex)
    {
        skillList.GetChild(chosenSkillIndex).GetComponent<Image>().color = Color.green;
    }

    [ObserversRpc]
    private void SkillChosenRpc(Skill chosenSkill)
    {
        if (chosenSkill != money)
        {
            if (!ownedSkills.Contains(chosenSkill))
            {
                ownedSkills.Add(chosenSkill);
                availableSkills.Remove(chosenSkill);
            }
            else
            {
                chosenSkill.AddLevel();
            }
        }
        Player.Instance.HandleSkillSelection(chosenSkill);
        chosen = 0;
        votes.Clear();
        upgradeUI.SetActive(false);
        Player.Instance.ChangePlayerControlsStatus(true);
    }

    [ObserversRpc]
    private void StartSkillChoose()
    {
        Player.Instance.ChangePlayerControlsStatus(false);
        foreach (Transform skillSlot in skillList)
        {
            skillSlot.GetComponent<Button>().interactable = true;
            skillSlot.GetComponent<Image>().color = Color.white;
            skillSlot.gameObject.SetActive(false);
        }
    }

    [Server]
    private void OnNewLevel()
    {
        StartSkillChoose();
        GameManager.Instance.ChangeEnemiesStatus(false);          
        StartCoroutine(WaitForSkillChoice());
    }

    [ObserversRpc]
    private void ShowSkills(int seed)
    {
        Random.InitState(seed);
        List<Skill> tempAvailableSkills = new List<Skill>(availableSkills);

        int i;
        for (i = 0; i < skillList.childCount; i++)
        {
            Transform skillSlot = skillList.GetChild(i);
            skillSlot.GetComponent<Button>().onClick.RemoveAllListeners();

            if (ownedSkills.Count < maxPassiveSkills + maxActiveSkills)
            {
                List<Skill> tempList = ownedSkills.FindAll(x => x.GetLevel() < x.levels.Length).ToList();
                if (tempList.Count > 0)
                {
                    if (ChooseSkillFromOwnedSkills(tempList, skillSlot, tempAvailableSkills))
                        continue;
                }
                else if (tempAvailableSkills.Count > 0)
                {
                    if (ChooseSkillFromAvailable(tempAvailableSkills, skillSlot))
                        continue;
                }
            }
            AddMoneySkill(skillSlot);
            break;
        }
        ActivateSkillSlots(i);
    }

    private bool ChooseSkillFromOwnedSkills(List<Skill> ownedSkillsToUpgrade, Transform skillSlot, List<Skill> availableSkills)
    {
        int luck = 1; //player stat
        float ownedChance = 1 / (1 + luck);
        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= ownedChance)
        {
            int random1 = Random.Range(0, ownedSkillsToUpgrade.Count);
            int random2 = Random.Range(0, ownedSkillsToUpgrade.Count);

            if (random1 != random2)
            {
                Skill randomOwnedSkill = ownedSkillsToUpgrade[random1];
                SetSkillSlot(randomOwnedSkill, skillSlot);
                return true;
            }
            else
            {
                return ChooseSkillFromAvailable(availableSkills, skillSlot);
            }
        }
        else return ChooseSkillFromAvailable(availableSkills, skillSlot);
    }

    private bool ChooseSkillFromAvailable(List<Skill> availableSkills, Transform skillSlot)
    {
        List<Skill> tempAvailableSkills = new List<Skill>(availableSkills);
        int countPassiveSkills = ownedSkills.Count(x => !x.isActive);
        int countActiveSkills = ownedSkills.Count(x => x.isActive);

        while (tempAvailableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, tempAvailableSkills.Count);
            Skill selectedSkill = tempAvailableSkills[randomIndex];

            if ((selectedSkill.isActive && countActiveSkills < maxActiveSkills)
                || (!selectedSkill.isActive && countPassiveSkills < maxPassiveSkills))
            {
                SetSkillSlot(selectedSkill, skillSlot);
                availableSkills.RemoveAt(availableSkills.IndexOf(selectedSkill));
                return true;
            }

            tempAvailableSkills.RemoveAt(randomIndex);
        }

        return false;
    }

    private void SetSkillSlot(Skill skill, Transform skillSlot)
    {
        skillSlot.GetComponent<SkillSlot>().SetSlot(skill);
        skillSlot.GetComponent<Button>().onClick.AddListener(delegate { VoteForSkill(skill); });
        votes.Add(skill, 0);
    }

    private void AddMoneySkill(Transform skillSlot)
    {
        skillSlot.GetComponent<SkillSlot>().SetSlot(money);
        skillSlot.GetComponent<Button>().onClick.AddListener(delegate { VoteForSkill(money); });
        votes.Add(money, 0);
    }

    private void VoteForSkill(Skill skill)
    {     
        ServerVote(skill);
        foreach (Transform skillSlot in skillList)
        {
            skillSlot.GetComponent<Button>().interactable = false;
        }
    }

    [ObserversRpc]
    private void ActivateSkillSlots(int index)
    {
        for (int i = 0; i <= index; i++)
        {
            skillList.GetChild(i).gameObject.SetActive(true);
        }
        upgradeUI.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerVote(Skill skill)
    {
        Skill s = votes.First(x => x.Key.skillName == skill.skillName).Key;
        votes[s]++;
        chosen++;
    }

    private (Skill, int) ChooseSkill()
    {
        Skill chosenSkill = null;
        int chosenIndex = -1;

        List<Skill> maxVotedSkills = new List<Skill>();
        int maxVotes = 0;

        foreach (KeyValuePair<Skill, int> pair in votes)
        {
            if (pair.Value > maxVotes)
            {
                maxVotes = pair.Value;
                maxVotedSkills.Clear();
                maxVotedSkills.Add(pair.Key);
            }
            else if (pair.Value == maxVotes && pair.Value != 0)
            {
                maxVotedSkills.Add(pair.Key);
            }
        }

        if (maxVotedSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, maxVotedSkills.Count);
            chosenSkill = maxVotedSkills[randomIndex];

            int index = 0;
            foreach (KeyValuePair<Skill, int> pair in votes)
            {
                if (pair.Key.skillName == chosenSkill.skillName)
                {
                    chosenIndex = index;
                    break;
                }
                index++;
            }
        }

        return (chosenSkill, chosenIndex);
    }
}
