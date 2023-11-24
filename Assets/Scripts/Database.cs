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

    public static Sprite GetSkillIcon(int skillIconIndex)
    {
        if (skillIconIndex >= 0 && skillIconIndex < Instance.data.skilIcons.Length)
        {
            return Instance.data.skilIcons[skillIconIndex];
        }
        return null;
    }
}

