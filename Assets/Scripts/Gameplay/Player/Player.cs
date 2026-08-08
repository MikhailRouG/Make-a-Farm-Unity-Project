using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    public class Player : NetworkBehaviour
    {
        private PlayerInputActions _input;
        private PlayerMove _move;
        private PlayerCameraController _cameraController;
        private PlayerInteraction _interaction;
        private PlayerInventory _playerInventory;
        private PlayerPlacement _placement;

        private bool _jumpPressed;
        private bool _inputEnabled = true;
        private bool _inputBound;

        public event Action onUseItem;
        public event Action onEsc;

        private void Awake()
        {
            _input = new PlayerInputActions();
            _move = GetComponent<PlayerMove>();
            _interaction = GetComponent<PlayerInteraction>();
            _playerInventory = GetComponent<PlayerInventory>();
            _placement = GetComponent<PlayerPlacement>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            _cameraController = GetComponent<PlayerCameraController>();
            _cameraController.Initialize();
            _move.SetAimSource(_cameraController.AimTransform);

            BindInput();
        }

        private void OnEnable()
        {
            // Rebound here and not only in OnStartLocalPlayer, so that disabling
            // and re-enabling the object does not drop input for good.
            if (isLocalPlayer) BindInput();
        }

        private void OnDisable()
        {
            UnbindInput();
        }

        private void OnDestroy()
        {
            UnbindInput();

            _input?.Dispose();
            _input = null;
        }

        private void BindInput()
        {
            if (_inputBound || _input == null) return;
            _inputBound = true;

            _input.Player.Enable();

            _input.Player.Jump.performed += OnJumpPerformed;
            _input.Player.Jump.canceled += OnJumpCanceled;
            _input.Player.Interact.performed += OnInteract;
            _input.Player.UseItem.performed += OnUseItem;
            _input.Player.Escape.performed += OnEscape;
            _input.Player.Plant.performed += OnPlant;
            _input.Player.CursorVisable.performed += OnChangeCursor;
        }

        private void UnbindInput()
        {
            if (!_inputBound || _input == null) return;
            _inputBound = false;

            _input.Player.Jump.performed -= OnJumpPerformed;
            _input.Player.Jump.canceled -= OnJumpCanceled;
            _input.Player.Interact.performed -= OnInteract;
            _input.Player.UseItem.performed -= OnUseItem;
            _input.Player.Escape.performed -= OnEscape;
            _input.Player.Plant.performed -= OnPlant;
            _input.Player.CursorVisable.performed -= OnChangeCursor;

            _input.Player.Disable();
            _jumpPressed = false;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (!_inputEnabled) return;

            CursorHandle();

            Vector2 moveDir = _input.Player.Move.ReadValue<Vector2>();
            Vector2 lookDir = _input.Player.Look.ReadValue<Vector2>();
            float zoom = _input.Player.Zoom.ReadValue<float>();
            bool isRunning = _input.Player.Shift.IsPressed();

            // Order matters: camera first (movement is built from its direction in
            // this same frame), then gravity, movement and body rotation.
            if (_cameraController != null)
            {
                bool lookIsPointerDelta = _input.Player.Look.activeControl?.device is Pointer;
                if (Cursor.visible == false) _cameraController.HandleRotate(lookDir, lookIsPointerDelta);

                if (zoom != 0) _cameraController.HandleZoom(zoom);
            }

            _move.HandleGravityAndJump(_jumpPressed);
            _move.HandleMove(moveDir, isRunning);

            if (_cameraController != null)
            {
                if (_cameraController.FirstPerson) _move.RotationOnFirstPersonCamera();
                else _move.RotationOnThirdPersonCamera();
            }
        }

        public void SetInputEnabled(bool value)
        {
            _inputEnabled = value;
            if (!value && _move != null) _move.ResetMotion();
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx) => _jumpPressed = true;

        private void OnJumpCanceled(InputAction.CallbackContext ctx) => _jumpPressed = false;

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (!_inputEnabled) return;
            if (_interaction == null) return;

            _interaction.TryInteract();
        }

        private void OnUseItem(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (!_inputEnabled) return;
            if (_playerInventory == null) return;

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

            if (_playerInventory != null)
                _playerInventory.OnEscape();

            if (_placement != null && _placement.IsPlanting)
            {
                _placement.CancelPlacement();
                return;
            }

            onEsc?.Invoke();
        }

        private void OnPlant(InputAction.CallbackContext ctx)
        {
            if (!_inputEnabled) return;
            if (_placement != null && _placement.IsPlanting) _placement.ConfirmPlacement();
        }

        private void OnChangeCursor(InputAction.CallbackContext ctx)
        {
            Cursor.visible = !Cursor.visible;
        }

        private void CursorHandle()
        {
            if (_cameraController == null || _interaction == null) return;

            if (_cameraController.FirstPerson)
            {
                _interaction.InteractionByForward();
                return;
            }

            if (_input.Player.RightClick.IsPressed())
            {
                Cursor.visible = false;
                _interaction.InteractionByForward();
            }
            else
            {
                Cursor.visible = true;
                _interaction.InteractionByCursor();
            }
        }
    }
}
