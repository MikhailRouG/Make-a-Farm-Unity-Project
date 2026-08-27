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

        // Whether the player wants the cursor shown. Owned by the toggle key and by
        // camera-mode changes, never overwritten by the per-frame logic.
        private bool _cursorWanted = true;
        private bool _lastFirstPerson;

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

        // While a UI is open the cursor and the camera belong to UiManager: the player
        // must not fight it from Update, which runs after and would always win.
        private static bool UiOpen => UiManager.Instance != null && UiManager.Instance.HasOpenUi;

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (!_inputEnabled) return;

            bool uiOpen = UiOpen;

            if (!uiOpen)
                CursorHandle();

            Vector2 moveDir = _input.Player.Move.ReadValue<Vector2>();
            Vector2 lookDir = _input.Player.Look.ReadValue<Vector2>();
            float zoom = _input.Player.Zoom.ReadValue<float>();
            bool isRunning = _input.Player.Shift.IsPressed();

            if (_cameraController != null && !uiOpen)
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
            if (UiOpen) return;

            _interaction.TryInteract();
        }

        private void OnUseItem(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (!_inputEnabled) return;
            if (_playerInventory == null) return;

            // Input actions fire even when the click landed on a button, so without
            // this a click inside the shop would hide the cursor and use an item.
            if (UiOpen) return;

            // Through the wish, not straight into Cursor: a direct write would be
            // undone by CursorHandle on the very next frame.
            _cursorWanted = false;
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

        // Always flips, whatever else is going on: no UI check, and CursorHandle reads
        // _cursorWanted instead of deciding on its own, so it cannot undo the press.
        private void OnChangeCursor(InputAction.CallbackContext ctx)
        {
            _cursorWanted = !_cursorWanted;
            Cursor.visible = _cursorWanted;
        }

        private void CursorHandle()
        {
            if (_cameraController == null || _interaction == null) return;

            bool firstPerson = _cameraController.FirstPerson;

            // Switching camera mode resets the wish to that mode's default, otherwise
            // a toggle made in third person would follow you into first person.
            if (firstPerson != _lastFirstPerson)
            {
                _lastFirstPerson = firstPerson;
                _cursorWanted = !firstPerson;
            }

            // Holding right mouse means "I am looking around": the cursor hides for as
            // long as it is held and comes back on release, without touching the wish.
            bool lookingAround = !firstPerson && _input.Player.RightClick.IsPressed();
            bool visible = _cursorWanted && !lookingAround;

            Cursor.visible = visible;

            if (visible)
                _interaction.InteractionByCursor();
            else
                _interaction.InteractionByForward();
        }
    }
}
