using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private Powerup[] listOfPowerups;
    [SerializeField] private Transform powerupsContainer;
    [SerializeField] private GameObject powerupInfo;

    private void Start()
    {
        powerupInfo.SetActive(false);
        foreach (var powerup in listOfPowerups)
        {
            string fieldName = Enum.GetName(typeof(PassiveItemType), powerup.type);
            if (!string.IsNullOrEmpty(fieldName))
            {
                FieldInfo field = typeof(PlayerData).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null && field.FieldType == typeof(int))
                {
                    powerup.level = (int)field.GetValue(CloudData.PlayerData);
                }
            }
        }

        SetUpShop();
    }

    private void SetUpShop()
    {
        for (int i = 0; i < listOfPowerups.Length; i++)
        {
            powerupsContainer.GetChild(i).GetComponent<PowerupUIElement>().SetUp(listOfPowerups[i]);
            int j = i;
            powerupsContainer.GetChild(i).GetComponentInChildren<Button>().onClick.AddListener(delegate { OpenInfo(listOfPowerups[j], powerupsContainer, j); });
        }
    }

    private void OpenInfo(Powerup powerup, Transform powerupsContainer, int index)
    {
        powerupInfo.GetComponent<PowerupInfo>().SetUp(powerup, powerupsContainer, index);
        powerupInfo.gameObject.SetActive(true);
    }
}
