using System.Linq;
using UnityEngine;

public class Database : MonoBehaviour
{
    public static Database Instance { get; private set; }
    public DatabaseSO data;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Sprite GetItemIcon(int itemIconIndex)
    {
        if (itemIconIndex >= 0 && itemIconIndex < Instance.data.itemIcons.Length)
        {
            return Instance.data.itemIcons[itemIconIndex];
        }
        return null;
    }

    public static Weapon GetWeaponByName(string name)
    {
        return Instance.data.weapons.FirstOrDefault(x => x.itemName == name);
    }
}

