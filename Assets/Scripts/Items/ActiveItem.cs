using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveItem", menuName = "ScriptableObjects/Items/ActiveItem")]
public class ActiveItem : Item
{
    public GameObject prefab;
    public ActiveItemLevel[] levels;
    public ActiveItemLevel GetCurrentLevel() 
    { 
        return levels[GetLevel()]; 
    }
    public string GetNextLevelDescription()
    {
        return levels[GetLevel() + 1].Description;
    }
}

[Serializable]
public class ActiveItemLevel
{
    [SerializeField]
    [TextArea(4, 6)] private string description;
    public string Description { get => description; private set => description = value; }
    [field: SerializeField]
    public float Damage { get; private set; }
    [field: SerializeField]
    public float Range { get; private set; }
    [field: SerializeField]
    public float Cooldown { get; private set; }
    [field: SerializeField]
    public float Speed { get; private set; }
    [field: SerializeField]
    public int Projectiles { get; private set; }
    [field: SerializeField]
    public int Pierce { get; private set; }
}
