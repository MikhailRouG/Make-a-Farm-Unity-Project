using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEnterExitVehicle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject playerCamera;
    private CharacterController playerController;
    private PlayerMove playerMove;
    private PlayerInput playerInput;

    private bool inVehicle = false;
    private Transform seatPoint;
    private VehicleHandler vehicleHandler;
    private InputAction exitAction;
    private float seatTime, delayTime = 0.5f;
    private void Awake()
    {
        playerController = GetComponent<CharacterController>();
        playerMove = GetComponent<PlayerMove>();
        playerInput = GetComponent<PlayerInput>();
        exitAction = playerInput.actions["Interact"];
    }

    private void OnEnable()
    {
        exitAction.performed += TryExitVehicle;
    }

    private void OnDisable()
    {
        exitAction.performed -= TryExitVehicle;
    }

    public bool EnterVehicle(VehicleHandler vehicle)
    {
        if (inVehicle || Time.time - seatTime < delayTime) return false;

        inVehicle = true;
        vehicleHandler = vehicle;
        seatPoint = vehicle.SeatPoint;

        transform.position = seatPoint.position;
        transform.rotation = seatPoint.rotation;
        transform.SetParent(vehicle.gameObject.transform);

        playerController.enabled = false;
        playerMove.enabled = false;
        playerCamera.gameObject.SetActive(false);
        seatTime = Time.time;
        return true;
    }

    private void TryExitVehicle(InputAction.CallbackContext ctx)
    {
        if (!inVehicle || Time.time - seatTime < delayTime) return;

        transform.SetParent(null);
        // Exit to the right of the car
        Vector3 exitOffset = vehicleHandler.transform.right * -2f;
        transform.position = seatPoint.position + exitOffset;
        transform.rotation = Quaternion.Euler(0f, vehicleHandler.transform.eulerAngles.y, 0f);

        inVehicle = false;
        seatPoint = null;

        // Turn the player back on
        playerCamera.gameObject.SetActive(true);
        playerController.enabled = true;
        playerMove.enabled = true;

        vehicleHandler.ExitVehicle();
        seatTime = Time.time;
    }
}