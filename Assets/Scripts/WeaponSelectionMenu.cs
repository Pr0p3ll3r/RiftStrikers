using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponMenuSelection : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    private Weapon[] weapons;
    private int currentIndex = 0;

    private void Start()
    {
        weapons = Database.Instance.data.weapons;
        nextButton.onClick.AddListener(NextWeapon);
        previousButton.onClick.AddListener(PreviousWeapon);
        UpdateUI();
    }

    private void UpdateUI()
    {
        weaponImage.sprite = weapons[currentIndex].icon;
        string weaponName = weapons[currentIndex].itemName;
        weaponText.text = weaponName;
        WeaponSelection.SelectedWeapon = Database.GetWeaponByName(weaponName);
    }

    private void NextWeapon()
    {
        currentIndex = (currentIndex + 1) % weapons.Length;
        UpdateUI();
    }

    private void PreviousWeapon()
    {
        currentIndex = (currentIndex - 1 + weapons.Length) % weapons.Length;
        UpdateUI();
    }
}
