using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupUIElement : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI level;

    public void SetUp(Powerup powerup)
    {
        icon.sprite = powerup.icon;
        level.text = $"Lvl: {powerup.level}";
    }
}
