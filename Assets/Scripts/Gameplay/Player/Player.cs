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
        private bool _inputEnabled = true;
        public event Action onUseItem;
        public event Action onEsc;

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }
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
            input.Player.CursorVisable.performed += OnChangeCursor;
        }
        private void OnDisable()
        {
            if (isLocalPlayer && input != null)
            {
                input.Player.Interact.performed -= OnInteract;
                input.Player.UseItem.performed -= OnUseItem;
                input.Player.Escape.performed -= OnEscape;
                input.Player.Plant.performed -= OnPlant;
                input.Player.CursorVisable.performed -= OnChangeCursor;

                input.Player.Disable();
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (!_inputEnabled) return;
            CursorHandle();

            Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
            Vector2 lookDir = input.Player.Look.ReadValue<Vector2>();
            float zoom = input.Player.Zoom.ReadValue<float>();
            bool isRunning = input.Player.Shift.IsPressed();

            move.HandleMove(moveDir, isRunning);
            if (cameraController != null)
            {
                if (cameraController.FirstPerson) move.RotationOnFirstPersonCamera();
                else move.RotationOnThirdPersonCamera();
                if (Cursor.visible == false) cameraController.HandleRotate(lookDir);

               if (zoom != 0) cameraController.HandleZoom(zoom);
            }
            move.HandleGravityAndJump(jumpPressed);
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (!_inputEnabled) return;
            interaction.TryInteract();
        }

        private void OnUseItem(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (!_inputEnabled) return;
            if (playerInventory == null) return;
            Cursor.visible = false;
            onUseItem?.Invoke();
        }

        private void OnEscape(InputAction.CallbackContext ctx)
        {
            if (UiManager.Instance != null && UiManager.Instance.HasOpenUi)
            {
                UiManager.Instance.CloseTopUi();
                return;
            }

            playerInventory.OnEscape();

            if (_placement._isPlanting)
            {
                _placement.CancelPlacement();
                return;
            }

            onEsc?.Invoke();
        }

        private void OnPlant(InputAction.CallbackContext ctx)
        {
            if (!_inputEnabled) return;
            if (_placement._isPlanting) _placement.ConfirmPlacement();
        }

        private void CursorHandle()
        {
            bool rightClick;
            if (cameraController.FirstPerson)
            {
                interaction.InteractionByForward();
                return;
            }
            else rightClick = input.Player.RightClick.IsPressed();
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
        }

        private void OnChangeCursor(InputAction.CallbackContext ctx)
        {
            Cursor.visible = !Cursor.visible;
        }
    }
}