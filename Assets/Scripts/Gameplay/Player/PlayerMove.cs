using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMove : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float shiftSpeed = 7f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 55f;
        [SerializeField, Range(0f, 1f)] private float airControl = 0.35f;

        [Header("Rotation")]
        [SerializeField] private float firstPersonRotationSpeed = 25f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedGravity = -2f;

        [Header("Animation")]
        [SerializeField] private float animationSmoothing = 12f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private CharacterController controller;
        private Animator animator;
        private Transform cameraTransform;

        private Vector3 planarVelocity;
        private Vector3 moveInput;
        private float yVelocity;
        private bool hasJumpParameter;

        private Vector3 lastPosition;
        private float animatorSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            hasJumpParameter = HasParameter(JumpHash);
            lastPosition = transform.position;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (cameraTransform != null) return;

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

        // Movement direction comes from the camera rig rather than Camera.main:
        // the Cinemachine camera is damped, which makes the direction drift.
        public void SetAimSource(Transform aim)
        {
            if (aim != null) cameraTransform = aim;
        }

        // Animation runs on every client, not just the owner: speed is measured
        // from the actual transform displacement, so for remote players it matches
        // the NetworkTransform interpolation and the feet do not slide.
        private void Update()
        {
            if (animator == null) return;

            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            delta.y = 0f;

            float measured = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            measured = Mathf.Min(measured, shiftSpeed); // clamp the spike from a teleport/respawn

            animatorSpeed = Mathf.Lerp(animatorSpeed, measured, SmoothFactor(animationSmoothing));
            animator.SetFloat(SpeedHash, animatorSpeed);
        }

        // Clears the accumulated velocity when input is taken away (open UI and the
        // like), so control does not come back with the character already at speed.
        public void ResetMotion()
        {
            planarVelocity = Vector3.zero;
            moveInput = Vector3.zero;
        }

        public void HandleMove(Vector2 direction, bool isRunning)
        {
            if (!isLocalPlayer || !controller.enabled) return;

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;
            }
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveInput = forward * direction.y + right * direction.x;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            float currentSpeed = isRunning ? shiftSpeed : speed;
            Vector3 targetVelocity = moveInput * currentSpeed;

            // Acceleration/deceleration instead of an instant speed change: the
            // network gets a continuous curve and snapshot interpolation stays smooth.
            float rate = targetVelocity.sqrMagnitude > planarVelocity.sqrMagnitude
                ? acceleration
                : deceleration;
            if (!controller.isGrounded) rate *= airControl;
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, rate * Time.deltaTime);

            Vector3 velocity = planarVelocity;
            velocity.y = yVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        // Call before HandleMove, or the vertical velocity is applied a frame late.
        public void HandleGravityAndJump(bool jumpPressed)
        {
            if (!isLocalPlayer || !controller.enabled) return;

            if (controller.isGrounded)
            {
                if (yVelocity < 0f)
                    yVelocity = groundedGravity;

                if (jumpPressed)
                {
                    yVelocity = jumpForce;
                    if (animator != null && hasJumpParameter) animator.SetTrigger(JumpHash);
                }
            }

            yVelocity += gravity * Time.deltaTime;
        }

        public void RotationOnFirstPersonCamera()
        {
            if (!isLocalPlayer || cameraTransform == null) return;

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;

            // The body is not snapped to the mouse direction: mouse jitter would
            // otherwise reach other clients as rotation stutter.
            Quaternion target = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, SmoothFactor(firstPersonRotationSpeed));
        }

        // Framerate-independent smoothing: the rotation reaches its target in the
        // same wall-clock time at 30 and at 200 FPS.
        private static float SmoothFactor(float sharpness) =>
            1f - Mathf.Exp(-sharpness * Time.deltaTime);

        private bool HasParameter(int nameHash)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == nameHash) return true;
            }
            return false;
        }
    }
}
