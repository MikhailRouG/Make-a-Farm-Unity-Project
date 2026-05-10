using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class Player : NetworkBehaviour
{
    public Inventory Inventory { get; private set; }


    private PlayerInputActions input;
    private PlayerMove move;
    private PlayerInteraction interaction;
    private PlayerPlacement _placement;
    private PlayerInventory playerInventory;

    private bool jumpPressed;
    private bool interactPressed;
    private bool cursorTogglePressed;

    private void Awake()
    {
        input = new PlayerInputActions();
        Inventory = GetComponent<Inventory>();
        move = GetComponent<PlayerMove>();
        interaction = GetComponent<PlayerInteraction>();
        playerInventory = GetComponent<PlayerInventory>();
        _placement = GetComponent<PlayerPlacement>();
    }

    private void OnEnable()
    {
        input.Player.Enable();

        input.Player.Jump.performed += ctx => jumpPressed = true;
        input.Player.Jump.canceled += ctx => jumpPressed = false;

        input.Player.Interact.performed += OnInteract;

        input.Player.Interact.performed += ctx => interactPressed = true;
        input.Player.CursorVisable.performed += ctx => cursorTogglePressed = true;

        input.Player.UseItem.performed += OnUseItem;
        input.Player.Escape.performed += OnEscape;
    }

    private void OnDisable()
    {
        input.Player.Interact.performed -= OnInteract;
        input.Player.UseItem.performed -= OnUseItem;

        input.Player.Disable();
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
        if (Cursor.visible == false) move.HandleCamera(lookDir, zoom); 
        else if (zoom != 0) move.HandleCamera(Vector2.zero, zoom);
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
        if(playerInventory == null) return;
        playerInventory.CmdUseSelectedItem();
    }

    private void OnEscape(InputAction.CallbackContext ctx)
    {
        playerInventory.OnEscape();
    }

    public void Plant(int id)
    {
        _placement.enabled = true;
        _placement.StartPlanting(id);
    }

    public void PlantConfirm()
    {
        _placement.CmdConfirmPlacement();
    }

    public void PlantCancel()
    {
        _placement.enabled = false;
    }
}