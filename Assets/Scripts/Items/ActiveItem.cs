using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveItem", menuName = "ScriptableObjects/Items/ActiveItem")]
public class ActiveItem : Item
{
    public GameObject prefab;
    public ActiveItemLevel[] levels;
    public ActiveItemLevel GetCurrentLevel() { return levels[GetLevel()]; }
    public string GetNextLevelDescription()
    {
        return levels[GetLevel() + 1].description;
    }
}

[Serializable]
public class ActiveItemLevel
{
    [TextArea(4, 6)] public string description;
    public float damage;
    public float range;
    public float cooldown;
    public float speed;
    public int projectiles;
    public int pierce;
}
