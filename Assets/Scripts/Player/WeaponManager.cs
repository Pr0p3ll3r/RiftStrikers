using UnityEngine;
using System.Collections;
using FishNet.Object;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class WeaponManager : NetworkBehaviour
{
    public GameObject currentWeapon;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private int selectedWeapon = -1;
    public Weapon currentWeaponData;
    public Weapon testWeapon;
    [SerializeField] private LayerMask canBeShot;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource weaponSound;
    [SerializeField] private AudioClip equipSound;
    public bool isReloading = false;
    public bool isEquipping = false;

    private float currentCooldown;
    private PlayerInput playerInput;
    private InputAction fireAction;
    private PlayerHUD hud;
    private PlayerController controller;
    private Animator animCharacter;
    private Coroutine equip;
    private Coroutine reload;
    [SerializeField] private float bulletEjectingSpeed = 0.5f;

    private void Start()
    {
        hud = GetComponent<PlayerHUD>();
        animCharacter = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions["Fire"];
        StartEquip(0);
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (currentWeapon != null)
        {
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon != null && !isEquipping)
            {
                if (currentWeaponData.firingMode == FiringMode.SemiAuto)
                {
                    if (fireAction.WasPressedThisFrame() && currentCooldown <= 0 && !isReloading && !controller.IsRolling)
                    {
                        if (currentWeaponData.FireBullet())
                        {
                            ShootServer();
                            currentCooldown = currentWeaponData.fireRate;
                        }
                        else if (currentWeaponData.OutOfAmmo())
                            reload = StartCoroutine(Reload());
                    }
                }
                else
                {
                    if (fireAction.IsPressed() && currentCooldown <= 0 && !isReloading && !controller.IsRolling)
                    {
                        if (currentWeaponData.FireBullet())
                        {
                            ShootServer();
                            currentCooldown = currentWeaponData.fireRate;
                        }                         
                        else if (currentWeaponData.OutOfAmmo())
                            reload = StartCoroutine(Reload());
                    }
                }
            }
        }        
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            if (currentWeaponData.OutOfAmmo() && !isReloading) reload = StartCoroutine(Reload());
    }

    private void StartEquip(int index)
    {
        if (selectedWeapon == index)
            return;

        if (isReloading)
        {
            StopReload();
        }

        if (equip != null)
            StopCoroutine(equip);
        equip = StartCoroutine(Equip(index));
    }

    IEnumerator Equip(int index)
    {
        selectedWeapon = index;
        currentWeaponData = (Weapon)testWeapon.GetCopy();

        isEquipping = true;
        isReloading = false;

        ShowWeapon();
        controller.SetSpeed(currentWeaponData.movementSpeed);
        animCharacter.SetInteger("Weapon", (int)currentWeaponData.animSet);
        weaponSound.Stop();
        weaponSound.PlayOneShot(equipSound);

        if (currentWeaponData.equipSound != null)
        {
            weaponSound.clip = currentWeaponData.equipSound;
            weaponSound.Play();
        }

        hud.RefreshAmmo(currentWeaponData.GetClip());

        yield return new WaitForSeconds(1f);
        isEquipping = false;
    }

    public void ShowWeapon()
    {
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            weaponHolder.GetChild(i).transform.gameObject.SetActive(false);
        }

        if (currentWeaponData == null)
        {
            currentWeapon = null;
            return;
        }

        currentWeapon = weaponHolder.GetChild(currentWeaponData.childNumber).gameObject;
        currentWeapon.SetActive(true);
    }

    [ServerRpc]
    void ShootServer()
    {
        hud.RefreshAmmo(currentWeaponData.GetClip());

        //slide sound
        if (currentWeaponData.slideSound != null)
        {
            sfx.clip = currentWeaponData.slideSound;
            sfx.PlayOneShot(sfx.clip);
        }

        for (int i = 0; i < Mathf.Max(1, currentWeaponData.pellets); i++)
        {
            //bloom
            Vector3 bloom = transform.position + transform.forward * 1000f;
            bloom += Random.Range(-currentWeaponData.bloom, currentWeaponData.bloom) * transform.up;
            bloom += Random.Range(-currentWeaponData.bloom, currentWeaponData.bloom) * transform.right;
            bloom -= transform.position;
            bloom.Normalize();

            RaycastHit hit;

            if (Physics.Raycast(transform.position, bloom, out hit, Mathf.Infinity, canBeShot))
            {
                //Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    GameObject blood = Instantiate(bloodPrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
                    blood.transform.LookAt(hit.point + hit.normal);
                    Spawn(blood);
                    hit.collider.gameObject.GetComponent<IDamageable>().TakeDamage(currentWeaponData.damage);
                }
                else
                {
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
                    bulletHole.transform.LookAt(hit.point + hit.normal);
                    Spawn(bulletHole);
                }            
            }         
        }
        ShootRpc();
    }

    [ObserversRpc]
    void ShootRpc()
    {
        //muzzle
        ParticleSystem muzzleFlash = weaponHolder.GetChild(currentWeaponData.childNumber).gameObject.GetComponentInChildren<ParticleSystem>();
        muzzleFlash.Play();

        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.pitch = 1 - currentWeaponData.pitchRandom + Random.Range(-currentWeaponData.pitchRandom, currentWeaponData.pitchRandom);
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //Bullet trail

        //Bullet Case Out
    }

    IEnumerator Reload()
    {
        weaponSound.Stop();

        isReloading = true;
        animCharacter.SetBool("Reload", true);
        if (currentWeapon.GetComponent<Animator>() != null)
            currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);

        if (currentWeaponData.inserting)
        {
            do
            {
                if (!currentWeaponData.OutOfAmmo())
                {
                    isReloading = false;
                    yield break;
                }
                hud.StartReload(currentWeaponData.reloadTime);
                PlayReloadSoundServer();              
                yield return new WaitForSeconds(currentWeaponData.reloadTime);
                currentWeaponData.Reload();
                hud.RefreshAmmo(currentWeaponData.GetClip());
            }
            while (currentWeaponData.GetClip() != currentWeaponData.clipSize);
        }
        else
        {
            hud.StartReload(currentWeaponData.reloadTime);
            PlayReloadSoundServer();
            yield return new WaitForSeconds(currentWeaponData.reloadTime);
            currentWeaponData.Reload();
            hud.RefreshAmmo(currentWeaponData.GetClip());
        }
        animCharacter.SetBool("Reload", false);
        isReloading = false;
    }

    [ServerRpc]
    private void PlayReloadSoundServer()
    {
        PlayReloadSoundRpc();
    }

    [ObserversRpc]
    private void PlayReloadSoundRpc()
    {
        weaponSound.PlayOneShot(currentWeaponData.reloadSound);
    }

    private void StopReload()
    {
        StopCoroutine(reload);
        hud.StopReload();
        animCharacter.SetBool("Reload", false);
        isReloading = false;
        weaponSound.Stop();
    }
}