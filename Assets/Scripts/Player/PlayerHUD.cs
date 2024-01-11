using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FishNet.Object;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private float fadeOutTime = 4f;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private GameObject itemListItemPrefab;
    private Slider healthBar;
    private Slider staminaBar;
    private TextMeshProUGUI ammo;
    private GameObject vignette;
    private Transform weapon;
    private Slider reloadingSlider;
    private Transform activeitemsContainer;
    private Transform passiveitemsContainer;

    private void Awake()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        //Bottom Left
        healthBar = GameObject.Find("HUD/Game/BottomLeftCorner/Health/HealthBar").GetComponent<Slider>();
        staminaBar = GameObject.Find("HUD/Game/BottomLeftCorner/Stamina/StaminaBar").GetComponent<Slider>();

        //Bottom Right
        ammo = GameObject.Find("HUD/Game/BottomRightCorner/Ammo/Amount").GetComponent<TextMeshProUGUI>();
        weapon = GameObject.Find("HUD/Game/BottomRightCorner/Weapon").transform;
        reloadingSlider = GameObject.Find("HUD/Game/BottomRightCorner/Ammo/ReloadingSlider").GetComponent<Slider>();
        activeitemsContainer = GameObject.Find("HUD/Game/BottomRightCorner/ActiveItems").transform;
        passiveitemsContainer = GameObject.Find("HUD/Game/BottomRightCorner/PassiveItems").transform;

        //Center
        vignette = GameObject.Find("HUD/Game/Vignette").gameObject;
    }

    private IEnumerator FadeToZeroAlpha()
    {
        vignette.GetComponent<CanvasGroup>().alpha = 0.5f;

        while (vignette.GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            vignette.GetComponent<CanvasGroup>().alpha -= (Time.deltaTime / fadeOutTime);
            yield return null;
        }
    }

    public void RefreshBars(float currentHealth)
    {
        healthBar.value = currentHealth;
    }

    public void RefreshAmmo(int currentAmmo)
    {
        ammo.text = currentAmmo.ToString();
    }

    public void ShowVignette()
    {
        StartCoroutine(FadeToZeroAlpha());
    }

    public IEnumerator StaminaRestore(float cooldown)
    {
        staminaBar.value = staminaBar.minValue;
        staminaBar.maxValue = cooldown;
        while (staminaBar.value != staminaBar.maxValue)
        {
            staminaBar.value += Time.deltaTime;
            yield return null;
        }
    }

    public void RefreshWeapon(Weapon weaponData)
    {
        weapon.GetChild(0).GetComponentInChildren<Image>().sprite = weaponData.icon;
    }

    public void AddItemUI(Item item)
    {
        GameObject itemGO = null;
        Debug.Log(item.itemName);
        if (item is ActiveItem)
            itemGO = Instantiate(itemListItemPrefab, activeitemsContainer);
        else if(item is PassiveItem)
            itemGO = Instantiate(itemListItemPrefab, activeitemsContainer);
        itemGO.transform.GetChild(0).GetComponent<Image>().sprite = Database.GetItemIcon(item.itemIconIndex);
    }

    private IEnumerator Reload(float time)
    {
        reloadingSlider.maxValue = time;
        reloadingSlider.value = time;
        while (reloadingSlider.value != 0)
        {
            reloadingSlider.value -= Time.deltaTime;
            yield return null;
        }
    }

    public void StartReload(float time)
    {
        StartCoroutine(Reload(time));
    }

    public void StopReload()
    {
        StopCoroutine(nameof(Reload));
        reloadingSlider.value = 0;
    }
}