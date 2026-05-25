
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public InputSystem_Actions inputs;
    private CharacterController controller;
    private CinemachineImpulseSource impulseSource;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;

    [Header("Jump")]
    public float jumpForce = 10f;
    private float verticalVelocity;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing;
    private bool canDash = true;
    private float dashTimer;

    [Header("Wall Jump")]
    public float wallCheckDistance = 1f;
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpVerticalForce = 10f;
    public float wallJumpCooldown = 1f;

    private bool isTouchingWall;
    private bool canWallJump = true;

    [Header("External Forces")]
    public float externalForceDecay = 5f;
    private Vector3 externalForce;

    [Header("Push Objects")]
    public float pushForce = 4f;

    [Header("Input")]
    [SerializeField] private Vector2 moveInput;

    private RaycastHit wallHit;

    private void Awake()
    {
        inputs = new();

        controller = GetComponent<CharacterController>();

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    private void OnEnable()
    {
        inputs.Enable();

        inputs.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        inputs.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        inputs.Player.Jump.performed += OnJump;

        inputs.Player.Sprint.performed += OnDash;
    }

    private void OnDisable()
    {
        inputs.Disable();

        inputs.Player.Jump.performed -= OnJump;

        inputs.Player.Sprint.performed -= OnDash;
    }

    private void Update()
    {
        CheckWall();

        OnMove();
    }

    private void OnMove()
    {
        transform.Rotate(Vector3.up * moveInput.x * rotationSpeed * Time.deltaTime);

        Vector3 moveDir = transform.forward * moveSpeed * moveInput.y;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (isDashing)
        {
            moveDir = transform.forward * dashForce * (dashTimer / dashDuration);

            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }

        moveDir += externalForce;

        externalForce = Vector3.Lerp( externalForce, Vector3.zero, externalForceDecay * Time.deltaTime);

        moveDir.y = verticalVelocity;

        controller.Move(moveDir * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(0.2f);
            }

            return;
        }

        if (isTouchingWall && canWallJump)
        {
            Vector3 wallJumpDir = wallHit.normal;

            externalForce =
                wallJumpDir * wallJumpHorizontalForce;

            verticalVelocity =
                wallJumpVerticalForce;

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(0.5f);
            }

            StartCoroutine(WallJumpCooldown());
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash)
            return;

        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        canDash = false;

        isDashing = true;

        dashTimer = dashDuration;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    IEnumerator WallJumpCooldown()
    {
        canWallJump = false;

        yield return new WaitForSeconds(wallJumpCooldown);

        canWallJump = true;
    }

    private void CheckWall()
    {
        isTouchingWall = Physics.Raycast(transform.position,transform.right,out wallHit,wallCheckDistance);

        if (!isTouchingWall)
        {
            isTouchingWall = Physics.Raycast(transform.position, -transform.right, out wallHit, wallCheckDistance
            );
        }

        if (isTouchingWall)
        {
            if (!wallHit.collider.CompareTag("Wall"))
            {
                isTouchingWall = false;
            }
        }

        Debug.DrawRay(transform.position,transform.right * wallCheckDistance,Color.red);

        Debug.DrawRay(transform.position,-transform.right * wallCheckDistance,Color.blue);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 pushDir = (hit.transform.position - transform.position).normalized;

        if (hit.rigidbody != null && hit.rigidbody.linearVelocity == Vector3.zero)
        {
            hit.rigidbody.AddForce( pushDir * pushForce, ForceMode.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Daño recibido: " + damage);

        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(1.5f);
        }
    }
}