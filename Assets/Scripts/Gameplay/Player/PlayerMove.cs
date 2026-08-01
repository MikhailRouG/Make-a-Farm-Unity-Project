using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMove : NetworkBehaviour
    {
       private Transform cameraTransform;
        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float shiftSpeed = 7f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController controller;
        private Animator animator;

        private Vector3 moveInput;
        private Vector3 rotationAxis;
        private float yVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Camera systemCam = FindFirstObjectByType<Camera>();
                if (systemCam != null)
                {
                    cameraTransform = systemCam.transform;
                }
            }
        }
        public void HandleMove(Vector2 direction, bool isRunning)
        {
            if (!isLocalPlayer || !controller.enabled || cameraTransform == null) return;
            if (!controller.enabled) return;
            Vector2 input = direction;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;

            moveInput = (forward * input.y + right * input.x).normalized;

            float currentSpeed = isRunning ? shiftSpeed : speed;
            Vector3 velocity = moveInput * currentSpeed;

            velocity.y = yVelocity;
            if (animator != null)
            {
                animator.SetFloat("Speed", moveInput.sqrMagnitude * currentSpeed);
            }
            controller.Move(velocity * Time.deltaTime);
        }

        public void HandleGravityAndJump(bool jumpPressed)
        {
            if (!isLocalPlayer) return;
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


        public void RotationOnFirstPersonCamera()
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
        public void RotationOnThirdPersonCamera()
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}