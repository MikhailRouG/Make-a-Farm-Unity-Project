using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System;
namespace Gameplay.Player
{
    public class Player : NetworkBehaviour
    {
        private PlayerInputActions input;
        private PlayerMove move;
        private PlayerCameraController cameraController;
        private PlayerInteraction interaction;
        private PlayerInventory playerInventory;
        private PlayerPlacement _placement;
        private bool jumpPressed;
        private bool interactPressed;
        private bool cursorTogglePressed;
        public event Action onUseItem;
        public event Action onEsc;
        private void Awake()
        {
            input = new PlayerInputActions();
            move = GetComponent<PlayerMove>();
            interaction = GetComponent<PlayerInteraction>();
            playerInventory = GetComponent<PlayerInventory>();
            _placement = GetComponent<PlayerPlacement>();

        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (!isLocalPlayer) return;
            input.Player.Enable();
            cameraController = GetComponent<PlayerCameraController>();
            cameraController.Initialize();
            input.Player.Jump.performed += ctx => jumpPressed = true;
            input.Player.Jump.canceled += ctx => jumpPressed = false;

            input.Player.Interact.performed += OnInteract;

            input.Player.Interact.performed += ctx => interactPressed = true;
            input.Player.CursorVisable.performed += ctx => cursorTogglePressed = true;

            input.Player.UseItem.performed += OnUseItem;
            input.Player.Escape.performed += OnEscape;
            input.Player.Plant.performed += OnPlant;
        }
        private void OnDisable()
        {
            if (isLocalPlayer && input != null)
            {
                input.Player.Interact.performed -= OnInteract;
                input.Player.UseItem.performed -= OnUseItem;
                input.Player.Escape.performed -= OnEscape;
                input.Player.Plant.performed -= OnPlant;
                input.Player.Disable();
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            bool rightClick = input.Player.RightClick.IsPressed();

            if (rightClick)
            {
                Cursor.visible = false;
                interaction.InteractionByForward();
            }
            else
            {
                Cursor.visible = true;
                interaction.InteractionByCursor();
            }
            Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
            Vector2 lookDir = input.Player.Look.ReadValue<Vector2>();
            float zoom = input.Player.Zoom.ReadValue<float>();
            bool isRunning = input.Player.Shift.IsPressed();

            move.HandleMove(moveDir, isRunning);
            if (cameraController != null)
            {
                if (Cursor.visible == false) cameraController.HandleCamera(lookDir, zoom);
                else if (zoom != 0) cameraController.HandleCamera(Vector2.zero, zoom);
            }
            move.HandleGravityAndJump(jumpPressed);
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            interaction.TryInteract();
        }

        private void OnUseItem(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (playerInventory == null) return;
            onUseItem?.Invoke();
        }

        private void OnEscape(InputAction.CallbackContext ctx)
        {
            onEsc?.Invoke();
            playerInventory.OnEscape();
            if (_placement.enabled == true) _placement.CancelPlacement();
        }

        private void OnPlant(InputAction.CallbackContext ctx)
        {
            if (_placement.enabled == true) _placement.ConfirmPlacement();
        }
    }
}