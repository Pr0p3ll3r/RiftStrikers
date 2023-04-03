using UnityEngine;

public enum FiringMode
{
    Auto,
    SemiAuto,
    Burst
}

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
    public FiringMode firingMode;
    public int damage;
    public int clipSize;
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
    public float bloom;
    [Range(0, 1)] public float mainFOV;
    public bool inserting;

    [Header("Sounds")]
    public AudioClip equipSound;
    public AudioClip slideSound;
    public AudioClip reloadSound;
    public AudioClip gunshotSound;
    public float pitchRandom;
    public float shotVolume;

    private int clip;

    public override void Initialize()
    {
        clip = clipSize;
    }

    public override Item GetCopy()
    {
        return Instantiate(this);
    }

    public bool FireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }

    public bool FireBurst()
    {
        if (clip >= 3)
        {
            clip -= 3;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        if (inserting)
        {
            clip += 1;
        }
        else
        {
            clip = clipSize;
        }
    }

    public bool OutOfAmmo()
    {
        if (clip <= 0)
            return true;
        else
            return false;
    }

    public int GetClip() { return clip; }
}