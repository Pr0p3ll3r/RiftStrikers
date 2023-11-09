using UnityEngine;

public enum AnimationSet
{
    Unarmed,
    Rifle,
    Pistol,
    Melee
}

[CreateAssetMenu(fileName = "Weapon", menuName = "HumanSurvivors/Items/Weapon")]
public class Weapon : Item
{
    public int damage;
    public int ammo;
    public int pellets;
    public float pelletsSpread;
    public float range;
    public float fireRate;
    public float bulletForce;
    public GameObject bulletPrefab;
    public float movementSpeed;
    public float reloadTime;
    public int childNumber;
    public AnimationSet animSet;
    [Range(0, 1)] public float mainFOV;
    public bool inserting;

    [Header("Sounds")]
    public AudioClip equipSound;
    public AudioClip reloadSound;
    public AudioClip gunshotSound;
    public float pitchRandom;
    public float shotVolume;

    private int currentAmmo;

    public override void Initialize()
    {
        currentAmmo = ammo;
    }

    public override Item GetCopy()
    {
        Item weapon = Instantiate(this);
        weapon.Initialize();
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
        if (inserting)
        {
            currentAmmo += 1;
        }
        else
        {
            currentAmmo = ammo;
        }
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