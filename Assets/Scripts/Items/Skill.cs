using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "HumanSurvivors/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public SkillType skillType;
    public SkillLevel[] levels;
    private int level;

    public virtual Skill GetCopy()
    {
        return this;
    }

    public int GetLevel() { return level; }
}

[Serializable]
public class SkillLevel
{
    [TextArea(4, 6)] public string description;
}

public enum SkillType
{
    Attack,
    Passive
}