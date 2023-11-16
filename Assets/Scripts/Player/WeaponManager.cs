using UnityEngine;
using System.Collections;
using FishNet.Object;
using UnityEngine.InputSystem;

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
    private GameObject closestEnemy;
    public GameObject ClosestEnemy => closestEnemy;

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

        closestEnemy = GetClosestEnemy();

        if (currentWeapon != null)
        {
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon != null && !isEquipping && currentCooldown <= 0 && !isReloading && !controller.IsRolling)
            {
                if (currentWeaponData.OutOfAmmo()) 
                    reload = StartCoroutine(Reload());
                else if (controller.AutoAim)
                {
                    if(closestEnemy)
                    {
                        Shoot();
                    }
                }
                else
                {
                    if (fireAction.IsPressed())
                    {
                        Shoot();
                    }
                }
            }
        }        
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed && IsOwner)
            if (!currentWeaponData.FullAmmo() && !isReloading) reload = StartCoroutine(Reload());
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
        currentWeaponData = testWeapon.GetCopy();

        isEquipping = true;
        isReloading = false;

        ShowWeapon();
        controller.SetSpeed(currentWeaponData.movementSpeed);
        animCharacter.SetInteger("Weapon", (int)currentWeaponData.animSet);
        weaponSound.Stop();
        weaponSound.PlayOneShot(equipSound);

        hud.RefreshAmmo(currentWeaponData.GetAmmo());

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

    void Shoot()
    {
        if (currentWeaponData.FireBullet())
        {
            ShootServer(currentWeaponData.damage, transform.position, transform.forward * 1000f, currentWeaponData.range, currentWeaponData.pellets);
            currentCooldown = currentWeaponData.fireRate;
            hud.RefreshAmmo(currentWeaponData.GetAmmo());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootServer(int damage, Vector3 postion, Vector3 direction, float range, int pellets)
    {
        //Debug.Log("ShootServer");

        for (int i = 0; i < Mathf.Max(1, pellets); i++)
        {
            if (Physics.Raycast(postion, direction, out RaycastHit hit, range, canBeShot) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                GameObject blood = Instantiate(bloodPrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
                blood.transform.LookAt(hit.point + hit.normal);
                Spawn(blood);
                hit.collider.gameObject.GetComponent<Enemy>().TakeDamage(damage);
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

    private GameObject GetClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, currentWeaponData.range, LayerMask.GetMask("Enemy"));
        GameObject closestEnemy = null;
        float minimumDistance = 1000000f;

        foreach (Collider enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < minimumDistance)
            {
                closestEnemy = enemy.gameObject;
                minimumDistance = distanceToEnemy;
            }
        }

        return closestEnemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, currentWeaponData.range);
    }

    IEnumerator Reload()
    {
        weaponSound.Stop();

        isReloading = true;
        animCharacter.SetBool("Reload", true);
        if (currentWeapon.GetComponent<Animator>() != null)
            currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);

        hud.StartReload(currentWeaponData.reloadTime);
        weaponSound.PlayOneShot(currentWeaponData.reloadSound);
        yield return new WaitForSeconds(currentWeaponData.reloadTime);
        currentWeaponData.Reload();
        hud.RefreshAmmo(currentWeaponData.GetAmmo());

        animCharacter.SetBool("Reload", false);
        isReloading = false;
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