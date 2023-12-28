using UnityEngine;
using System.Collections;
using FishNet.Object;
using UnityEngine.InputSystem;

public class WeaponManager : NetworkBehaviour
{
    public GameObject currentWeapon;
    [SerializeField] private Transform weaponHolder;
    public Weapon currentWeaponData;
    public Weapon testWeapon;
    [SerializeField] private LayerMask canBeShot;
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource weaponSound;
    [SerializeField] private AudioClip equipSound;
    public bool isReloading = false;
    public bool canReload = true;
    private float currentCooldown;
    private PlayerInput playerInput;
    private InputAction fireAction;
    private PlayerHUD hud;
    private PlayerController controller;
    private Animator animCharacter;
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
    }

    void Update()
    {
        if (!IsOwner || !Player.Instance.CanControl)
            return;

        if (currentWeapon != null)
        {
            closestEnemy = GameManager.Instance.GetClosestEnemy(transform.position, currentWeaponData.range);

            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon != null && currentCooldown <= 0 && !isReloading && !controller.IsRolling)
            {
                if (currentWeaponData.OutOfAmmo()) 
                    reload = StartCoroutine(Reload());
                else if (Player.Instance.AutoAim)
                {
                    if(closestEnemy)
                    {
                        Shoot();
                    }               
                }
                else if (fireAction.IsPressed())
                {   
                    Shoot();
                }
            }
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!IsOwner || !Player.Instance.CanControl)
            return;

        if (context.performed && !currentWeaponData.FullAmmo() && !isReloading)
            reload = StartCoroutine(Reload());
    }

    public void Equip(int index)
    {
        currentWeaponData = testWeapon.GetCopy();
        ShowWeapon();
        hud.RefreshWeapon(currentWeaponData);
        Player.Instance.CurrentMoveSpeed *= currentWeaponData.movementSpeedMultiplier;
        animCharacter.SetInteger("Weapon", (int)currentWeaponData.animSet);
        weaponSound.PlayOneShot(equipSound);
        hud.RefreshAmmo(currentWeaponData.GetAmmo());
    }

    private void ShowWeapon()
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

    private void Shoot()
    {
        if (currentWeaponData.FireBullet())
        {
            ShootServer(currentWeaponData.damage, transform.position, transform.forward * 1000f, currentWeaponData.range, currentWeaponData.pellets);
            currentCooldown = currentWeaponData.fireRate * Player.Instance.CurrentAttackCooldown;
            hud.RefreshAmmo(currentWeaponData.GetAmmo());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServer(float damage, Vector3 position, Vector3 direction, float range, int pellets)
    {
        //Debug.Log("ShootServer");

        for (int i = 0; i < Mathf.Max(1, pellets); i++)
        {
            if (Physics.Raycast(position, direction, out RaycastHit hit, range * Player.Instance.CurrentAttackRange, canBeShot))
            {             
                GameObject blood = Instantiate(bloodPrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
                blood.transform.LookAt(hit.point + hit.normal);
                Spawn(blood);
                hit.collider.gameObject.GetComponent<Enemy>().ServerTakeDamage(damage * Player.Instance.CurrentDamage);
            }
        }
        ShootRpc();
    }

    [ObserversRpc]
    private void ShootRpc()
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
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position, currentWeaponData.range);
    //}

    private IEnumerator Reload()
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

    public void StopReload()
    {
        if(reload != null) StopCoroutine(reload);
        hud.StopReload();
        animCharacter.SetBool("Reload", false);
        isReloading = false;
        weaponSound.Stop();
    }
}