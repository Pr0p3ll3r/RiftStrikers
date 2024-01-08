using FishNet.Component.Animating;
using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float gravity;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private float nextStep;
    [SerializeField] private float nextRoll;
    [SerializeField] private AnimationCurve rollingCurve;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance;

    private bool roll;
    public bool IsRolling => roll;
    public bool canRoll = true;

    private Player player;
    private Camera cam;
    private PlayerHUD hud;
    private CharacterController controller;
    private WeaponManager weaponManager;
    private AudioSource audioSource;
    private Animator animCharacter;
    private NetworkAnimator networkAnimator;
    private LineRenderer lineRenderer;
    private Vector3 lastPos;
    private Vector3 playerVelocity;
    private Vector2 moveInput;
    private float distMoved;
    private float lastRoll;
    private float rollTimer;
    private bool isGrounded;

    private void Start()
    {
        player = GetComponent<Player>();
        hud = GetComponent<PlayerHUD>();
        audioSource = GetComponent<AudioSource>();
        animCharacter = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        networkAnimator = GetComponent<NetworkAnimator>();
        weaponManager = GetComponent<WeaponManager>();
        lineRenderer = GetComponent<LineRenderer>();
        cam = Camera.main;

        Keyframe roll_LastFrame = rollingCurve[rollingCurve.length - 1];
        rollTimer = roll_LastFrame.time;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
            return;

        Camera.main.GetComponent<CameraFollow>().SetPlayer(transform);
    }

    private void Update()
    {
        if (!IsOwner || !player.CanControl)
            return;

        if (lastRoll > 0)
            lastRoll -= Time.deltaTime;

        if (player.AutoAim)
            lineRenderer.enabled = false;
        else
            lineRenderer.enabled = true;

        if (!roll)
            Move();    
        
        Look();
    }

    private void Move()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        controller.Move(player.currentMoveSpeed * Time.deltaTime * movement);

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        //animations
        Vector3 localMove = transform.InverseTransformDirection(movement);
        animCharacter.SetFloat("Speed", localMove.z, 0.05f, Time.deltaTime);
        animCharacter.SetFloat("Direction", localMove.x, 0.05f, Time.deltaTime);

        //footsteps
        distMoved += (transform.position - lastPos).magnitude;
        if (distMoved >= nextStep)
        {
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(footstepSound);
            distMoved = 0;
        }
        lastPos = transform.position;

        if (player.AutoAim)
        {
            if (weaponManager.ClosestEnemy)
            {
                Vector3 directionToEnemy = weaponManager.ClosestEnemy.transform.position - transform.position;
                directionToEnemy.y = 0;
                transform.forward = directionToEnemy;
            }
            else if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
            }
        }
    }

    private void Look()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (playerPlane.Raycast(ray, out float hit))
        {
            Vector3 hitPoint = ray.GetPoint(hit);
            var direction = hitPoint - transform.position;
            direction.y = 0;
            if (!player.AutoAim)
                transform.forward = direction;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hitPoint);
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!IsOwner || player.IsDead)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (!IsOwner || player.IsDead || !Player.Instance.CanControl)
            return;

        if (context.started && lastRoll <= 0)
            StartCoroutine(Roll());
    }

    public void OnDeath()
    {
        enabled = false;
        weaponManager.enabled = false;
    }

    private IEnumerator Roll()
    {
        weaponManager.StopReload();
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if(movement == Vector3.zero)
            yield break;
        bool makeSound = false;
        roll = true;
        float timer = 0;
        networkAnimator.SetTrigger("Roll");
        hud.StartCoroutine(hud.StaminaRestore(nextRoll));
        lastRoll = nextRoll;
        while (timer < rollTimer)
        {
            if (!makeSound && timer >= rollTimer/2)
            {
                audioSource.PlayOneShot(rollSound);
                makeSound = true;
            }              
            float speed = rollingCurve.Evaluate(timer);              
            transform.rotation = Quaternion.LookRotation(movement);
            controller.Move(speed * Time.deltaTime * movement);
            timer += Time.deltaTime;
            yield return null;
        }
        roll = false;
    }
}