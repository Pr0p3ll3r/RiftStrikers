using System;
using UnityEngine;

[Serializable]
public class Powerup
{
    public Sprite icon;
    public string powerupName;
    public PassiveItemType type;
    [TextArea(4, 6)] public string description;
    public int level;
    public int maxLevel;
    public int price;
    public float multiplier;
}
