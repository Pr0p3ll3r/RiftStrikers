using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed, gravity;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private float nextStep;
    [SerializeField] private float nextRoll;
    [SerializeField] private AnimationCurve rollingCurve;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance;

    public bool roll;

    private Camera cam;
    private CharacterController controller;
    private AudioSource audioSource;
    private Animator animCharacter;
    private Vector3 lastPos;
    private Vector3 movement;
    private Vector2 moveInput;
    private float distMoved;
    private float lastRoll;
    private float rollTimer;
    private float adjustedSpeed, adjustedNextStep;
    private bool isGrounded;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animCharacter = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        Keyframe roll_LastFrame = rollingCurve[rollingCurve.length - 1];
        rollTimer = roll_LastFrame.time;

        adjustedSpeed = speed;
        adjustedNextStep = nextStep;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (!Owner.IsLocalClient)
            return;

        Camera.main.GetComponent<CameraFollow>().SetPlayer(transform);
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (lastRoll > 0)
            lastRoll -= Time.deltaTime;

        if (!roll)
        {
            Move();
            Look();

            if (roll && lastRoll <= 0)
                StartCoroutine(Roll());
        }
    }

    void Move()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);      

        if (isGrounded)
        {
            movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            movement *= adjustedSpeed;          
        }
        movement.y += gravity * Time.deltaTime;
        controller.Move(movement * Time.deltaTime);

        //animations
        Vector3 localMove = transform.InverseTransformDirection(movement);
        animCharacter.SetFloat("Speed", localMove.z, 0.05f, Time.deltaTime);
        animCharacter.SetFloat("Direction", localMove.x, 0.05f, Time.deltaTime);

        //footsteps
        distMoved += (lastPos - transform.position).magnitude;
        lastPos = transform.position;

        if (distMoved > adjustedNextStep)
        {
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(footstepSound);

            distMoved = 0;
        }
    }

    void Look()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        float hit;
        //RaycastHit hit;
        if (playerPlane.Raycast(ray, out hit))
        //if (Physics.Raycast(ray, out hit, 100f, groundMask))
        {
            //Debug.DrawLine(ray.origin, hit.point);
            Vector3 hitPoint = ray.GetPoint(hit);
            Quaternion targetRotation = Quaternion.LookRotation(hitPoint - transform.position);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = targetRotation;

            //Debug.DrawLine(ray.origin, hit.point);
            //var targetRotation = hit.point - transform.position;
            //targetRotation.y = 0;
            //transform.forward = targetRotation;
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        if (context.started || context.performed)
        {
            roll = true;
        }
        else if(context.canceled)
        {
            roll = false;
        }
    }

    public void SetSpeed(float speedMultiplier)
    {
        adjustedSpeed = speed * speedMultiplier;
        adjustedNextStep = nextStep * speedMultiplier;
    }

    IEnumerator Roll()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if(movement == Vector3.zero)
            yield break;
        bool makeSound = false;
        roll = true;
        float timer = 0;
        animCharacter.SetTrigger("Roll");
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
            controller.Move(movement * speed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        roll = false;
    }

    public void Control(bool status)
    {
        enabled = status;
        controller.enabled = status;
    }
}