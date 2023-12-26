using UnityEngine;

public class Database : MonoBehaviour
{
    private static Database Instance;
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
}

