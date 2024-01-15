using UnityEngine;
using System.Collections;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using UnityEngine.UIElements;

public class WeaponManager : NetworkBehaviour
{
    public GameObject currentWeapon;
    [SerializeField] private Transform weaponHolder;
    public Weapon currentWeaponData;
    public Weapon testWeapon;
    [SerializeField] private LayerMask canBeShot;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource weaponSound;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private TrailRenderer bulletTrail;
    public bool isReloading = false;
    public bool canReload = true;
    private float currentCooldown;
    private PlayerInput playerInput;
    private InputAction fireAction;
    private PlayerHUD hud;
    private PlayerController controller;
    private Animator animCharacter;
    private Coroutine reload;
    private GameObject closestEnemy;
    public GameObject ClosestEnemy => closestEnemy;
    private ParticleSystem muzzleFlash;

    private void Awake()
    {
        hud = GetComponent<PlayerHUD>();
        animCharacter = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions["Fire"];
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(IsOwner)
            Equip(WeaponSelection.SelectedWeapon);
    }

    void Update()
    {
        if (!IsOwner || !Player.Instance.CanControl)
            return;

        if (currentWeapon != null)
        {
            if (Player.Instance.AutoAim) closestEnemy = GameManager.Instance.GetClosestEnemy(transform.position, currentWeaponData.range * Player.Instance.currentAttackRange);

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
                else if (!Pause.paused && fireAction.IsPressed())
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

    private void Equip(int index)
    {
        currentWeaponData = Database.Instance.data.weapons[index];
        currentWeaponData.Reload();       
        hud.RefreshWeapon(currentWeaponData);
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            weaponHolder.GetChild(i).transform.gameObject.SetActive(false);
        }
        currentWeapon = weaponHolder.GetChild(index).gameObject;
        currentWeapon.SetActive(true);
        ServerEquip(currentWeaponData.childNumber);
        hud.RefreshAmmo(currentWeaponData.GetAmmo());
        Player.Instance.currentMoveSpeed *= 1 + currentWeaponData.movementSpeedMultiplier / 100f;
        animCharacter.SetInteger("Weapon", (int)currentWeaponData.animSet);
        weaponSound.PlayOneShot(equipSound);    
    }

    [ServerRpc]
    private void ServerEquip(int childNumber)
    {
        RpcEquip(childNumber);
    }

    [ObserversRpc]
    private void RpcEquip(int childNumber)
    {
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            weaponHolder.GetChild(i).transform.gameObject.SetActive(false);
        }
        currentWeapon = weaponHolder.GetChild(childNumber).gameObject;
        currentWeapon.SetActive(true);
        currentWeaponData = Database.Instance.data.weapons[childNumber];
        muzzleFlash = currentWeapon.GetComponentInChildren<ParticleSystem>();
    }

    private void Shoot()
    {
        if (currentWeaponData.FireBullet())
        {
            Vector3 position;
            Vector3 direction;
            float range = currentWeaponData.range * Player.Instance.currentAttackRange;
            if (Player.Instance.AutoAim)
            {
                position = closestEnemy.transform.position - transform.position;
                direction = Vector3.ProjectOnPlane(position, Vector3.up.normalized);
                ShootServer(currentWeaponData.damage, transform.position, direction, range);
            }            
            else
            {
                position = transform.position;
                direction = transform.forward;
                ShootServer(currentWeaponData.damage, transform.position, direction, range);
            }

            if (Physics.Raycast(position, direction, out RaycastHit hit, range, canBeShot))
            {
                TrailRenderer trail = Instantiate(bulletTrail, muzzleFlash.transform.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit.point));
            }
            else
            {
                TrailRenderer trail = Instantiate(bulletTrail, muzzleFlash.transform.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, transform.position + transform.forward * range));
            }

            currentCooldown = currentWeaponData.fireRate * Player.Instance.currentAttackCooldown;
            hud.RefreshAmmo(currentWeaponData.GetAmmo());
            muzzleFlash.Play();
            sfx.clip = currentWeaponData.gunshotSound;
            sfx.volume = currentWeaponData.shotVolume;
            sfx.PlayOneShot(sfx.clip);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServer(float damage, Vector3 position, Vector3 direction, float range)
    {
        if (Physics.Raycast(position, direction, out RaycastHit hit, range, canBeShot))
        {
            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                enemy.ServerTakeDamage(damage * Player.Instance.currentDamage);
            }
            ShootRpc(muzzleFlash.transform.position, hit.point);
        }
        else
        {
            ShootRpc(muzzleFlash.transform.position, transform.position + transform.forward * range);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint)
    {
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= currentWeaponData.bulletForce * Time.deltaTime;
            yield return null;
        }
        trail.transform.position = hitPoint;
        Destroy(trail.gameObject);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ShootRpc(Vector3 startPoint, Vector3 hitPoint)
    {
        muzzleFlash.Play();

        TrailRenderer trail = Instantiate(bulletTrail, startPoint, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, hitPoint));

        sfx.clip = currentWeaponData.gunshotSound;
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);
    }

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