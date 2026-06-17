using Mirror;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class VehicleHandler : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private CarControl carControl;

    [SerializeField] private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private bool inVehicle = false;
    public string InteractionPrompt => "Enter Vehicle";
    public Transform SeatPoint => seatPoint;
    private void OnValidate()
    {
        carControl ??= GetComponent<CarControl>();
    }
    public void Initialize(PlayerInput input)
    {
        playerInput = input;
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
    }
    private void FixedUpdate()
    {
        if (!inVehicle) return;
            
        Vector2 move = moveAction.ReadValue<Vector2>();
        Vector2 look = lookAction.ReadValue<Vector2>();

        carControl.Move(move.x, move.y);
        carControl.HandleCameraLook(look);
    }
    public void CmdInteract(NetworkIdentity target)
    {
        //var enter = target.GetComponent<PlayerEnterExitVehicle>().EnterVehicle(this);

        //if (!enter) return;

        inVehicle = true;
        Initialize(target.GetComponent<PlayerInput>());

        ActivateCamera(true);
    }

    public void ExitVehicle()
    {
        playerInput.enabled = false;
        carControl.Move(0.0f,0.0f);
        ActivateCamera(false);
    }
    public void ActivateCamera(bool state)
    {
        if (_camera != null)
            _camera.gameObject.SetActive(state);
    }


}