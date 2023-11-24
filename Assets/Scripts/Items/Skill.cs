using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "HumanSurvivors/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public int skillIconIndex;
    public bool isActive;
    public SkillLevel[] levels;
    private int level;

    public Skill GetCopy()
    {
        return Instantiate(this);
    }

    public int GetLevel() { return level; }
    public void AddLevel() { level++; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Skill other = (Skill)obj;
        return skillName == other.skillName;
    }

    public override int GetHashCode()
    {
        return skillName.GetHashCode();
    }
}

[Serializable]
public class SkillLevel
{
    [TextArea(4, 6)] public string description;
}