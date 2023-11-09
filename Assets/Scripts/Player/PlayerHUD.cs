using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FishNet.Object;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private float fadeOutTime = 4f;
    [SerializeField] private Sprite emptyIcon;
    private Slider healthBar;
    private Slider staminaBar;
    private TextMeshProUGUI ammo;
    private TextMeshProUGUI levelText;
    private Slider expBar;
    private TextMeshProUGUI moneyText;
    private GameObject vignette;
    private Transform weaponParent;
    private Slider reloadingSlider;

    private void Awake()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        //Bottom Left
        moneyText = GameObject.Find("HUD/BottomLeftCorner/Money").GetComponent<TextMeshProUGUI>();
        healthBar = GameObject.Find("HUD/BottomLeftCorner/Health/HealthBar").GetComponent<Slider>();
        staminaBar = GameObject.Find("HUD/BottomLeftCorner/Stamina/StaminaBar").GetComponent<Slider>();

        //Bottom Right
        ammo = GameObject.Find("HUD/BottomRightCorner/Ammo/Amount").GetComponent<TextMeshProUGUI>();
        weaponParent = GameObject.Find("HUD/BottomRightCorner/Weapons").transform;
        reloadingSlider = GameObject.Find("HUD/BottomRightCorner/Ammo/ReloadingSlider").GetComponent<Slider>();

        //Exp Bar
        levelText = GameObject.Find("HUD/Exp/Level").GetComponent<TextMeshProUGUI>();
        expBar = GameObject.Find("HUD/Exp/ExpBar").GetComponent<Slider>();

        //Center
        vignette = GameObject.Find("HUD/Resources/Vignette").gameObject;
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

    public void RefreshBars(int currentHealth)
    {
        healthBar.value = currentHealth;
    }

    public void RefreshAmmo(int currentAmmo)
    {
        ammo.text = currentAmmo.ToString();
    }

    public void UpdateLevel(int level, int exp, int requireExp)
    {
        levelText.text = $"Level: {level} ({exp}/{requireExp})";
        float percentage = (float)exp / requireExp;
        expBar.value = percentage;
    }

    public void UpdateMoney(int money)
    {
        moneyText.text = $"${money}";
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