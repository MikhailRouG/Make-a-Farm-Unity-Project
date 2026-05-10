using Mirror;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : NetworkBehaviour
{
    [Header("Camera Settings")]
    private CinemachineThirdPersonFollow cameraSetting;
    [SerializeField] private Transform _cameraTrackTrnsform;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float _zoomSpeed = 50f;
    [SerializeField] private float clampAngle = 80f;

    private float xRotation;
    private float yRotation;
    private InputAction lookAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform model;
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float shiftSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Animator animator;
    // Runtime
    private Vector3 moveInput;
    private Vector3 rotationAxis;
    private float yVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        cameraSetting = GetComponentInChildren<CinemachineThirdPersonFollow>();
        xRotation = 0;
        yRotation = 0;
    }

    public void HandleMove(Vector2 direction, bool isRunning)
    {
        if (!controller.enabled) return;

        Vector2 input = direction;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;

        moveInput = (forward * input.y + right * input.x).normalized;

        float currentSpeed = isRunning ? shiftSpeed : speed;
        Vector3 velocity = moveInput * currentSpeed;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
            model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("Speed", velocity.sqrMagnitude);
        velocity.y = yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleGravityAndJump(bool jumpPressed)
    {
        if (controller.isGrounded)
        {
            if (yVelocity < 0)
                yVelocity = -2f;

            if (jumpPressed)
            {
                yVelocity = jumpForce;
                animator.SetTrigger("Jump");
            }
        }

        yVelocity += gravity * Time.deltaTime;
    }

    public void HandleCamera(Vector2 look, float zoom)
    {
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        yRotation += mouseX;

        if (_cameraTrackTrnsform != null)
            _cameraTrackTrnsform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (zoom != 0)
        {
            cameraSetting.CameraDistance -= zoom * _zoomSpeed * Time.deltaTime;
            cameraSetting.CameraDistance = Mathf.Clamp(cameraSetting.CameraDistance, 2, 15);
        }
    }
}