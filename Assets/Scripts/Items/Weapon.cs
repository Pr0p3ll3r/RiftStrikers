using UnityEngine;

public enum AnimationSet
{
    Unarmed,
    Rifle,
    Pistol,
    Melee
}

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Items/Weapon")]
public class Weapon : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public float damage;
    public int ammo;
    public int pellets;
    public float pelletsSpread;
    public float range;
    public float fireRate;
    public float bulletForce;
    public GameObject bulletPrefab;
    public float movementSpeedMultiplier;
    public float reloadTime;
    public int childNumber;
    public AnimationSet animSet;

    [Header("Sounds")]
    public AudioClip reloadSound;
    public AudioClip gunshotSound;
    public float pitchRandom;
    public float shotVolume;

    private int currentAmmo;

    public Weapon GetCopy()
    {
        Weapon weapon = Instantiate(this);
        weapon.Reload();
        return weapon;
    }

    public bool FireBullet()
    {
        if (currentAmmo > 0)
        {
            currentAmmo -= 1;
            return true;
        }
        else return false;
    }

    public bool FireBurst()
    {
        if (currentAmmo >= 3)
        {
            currentAmmo -= 3;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        currentAmmo = ammo;
    }

    public bool OutOfAmmo()
    {
        if (currentAmmo <= 0)
            return true;
        else
            return false;
    }

    public bool FullAmmo()
    {
        if (ammo == currentAmmo)
            return true;
        else
            return false;
    }

    public int GetAmmo() { return currentAmmo; }
}