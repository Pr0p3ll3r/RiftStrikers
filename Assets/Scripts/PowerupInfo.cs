using System.Reflection;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupInfo : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI powerupName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject errorScreen;

    private void Start()
    {
        errorScreen.SetActive(false);
    }

    public void SetUp(Powerup powerup, Transform powerupsContainer, int index)
    {
        icon.sprite = powerup.icon;
        powerupName.text = powerup.powerupName;
        description.text = powerup.description;
        price.text = $"${powerup.price}";
        if(powerup.level < powerup.maxLevel)
            upgradeButton.GetComponent<Button>().interactable = true;
        else
            upgradeButton.GetComponent<Button>().interactable = false;
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(delegate { Upgrade(powerup, powerupsContainer, index); });
    }

    private void Upgrade(Powerup powerup, Transform powerupsContainer, int index)
    {
        if (CloudData.PlayerData.Money < powerup.price)
        {
            errorScreen.SetActive(true);
            return;
        }
        CloudData.PlayerData.Money -= powerup.price;
        powerup.level += 1;
        if (powerup.level < powerup.maxLevel)
            upgradeButton.GetComponent<Button>().interactable = true;
        else
            upgradeButton.GetComponent<Button>().interactable = false;
        powerupsContainer.GetChild(index).GetComponent<PowerupUIElement>().SetUp(powerup);
        string fieldName = Enum.GetName(typeof(PassiveItemType), powerup.type);
        if (!string.IsNullOrEmpty(fieldName))
        {
            FieldInfo field = typeof(PlayerData).GetField(fieldName);

            if (field != null && field.FieldType == typeof(int))
            {
                int value = (int)field.GetValue(CloudData.PlayerData);
                field.SetValue(CloudData.PlayerData, value + 1);
            }
        }
        CloudData.Save();
    }
}
