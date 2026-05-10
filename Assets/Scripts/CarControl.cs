using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Car Settings")]
    public float enginePower = 2000f;
    public float brakePower = 100f;
    public float turnSpeed = 25f;
    public float turnSmoothness = 5f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float lookSensitivity = 80f;
    [SerializeField] private float maxLookX = 60f;
    [SerializeField] private float minLookX = -45f;
    [SerializeField] private float maxLookY = 60f;
    [SerializeField] private float minLookY = -45f;
    private float lookX, lookY;

    [Header("Car References")]
    public Transform[] wheels;       // Wheel Colliders
    public Transform[] wheelMeshes;  // Wheel Meshes
    public Transform centerOfMass;   // Center of Mass
    public GameObject steeringWheel; // Steering wheel mesh

    private Rigidbody rb;
    private float currentTurnAngle = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
    }
    public void HandleCameraLook(Vector2 look)
    {
        float mouseX = look.x * lookSensitivity/2 * Time.deltaTime;
        float mouseY = look.y * lookSensitivity * Time.deltaTime;
        lookX -= mouseY;
        lookY += mouseX;
        lookX = Mathf.Clamp(lookX, minLookX, maxLookX);
        lookY = Mathf.Clamp(lookY, minLookY, maxLookY);
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(lookX, lookY, 0f);
        }
    }

    public void Move(float h, float v)
    {
        float speed = rb.linearVelocity.magnitude;
        float steerLimit = Mathf.Lerp(turnSpeed, turnSpeed / 3f, speed / 50f);

        float targetTurnAngle = h * steerLimit;
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurnAngle, Time.deltaTime * turnSmoothness);

        // Visual steering wheel
        if (steeringWheel)
            steeringWheel.transform.localEulerAngles = new Vector3(-64, 0, Mathf.Clamp(currentTurnAngle * 3f, -90f, 90f));

        // Apply to wheels
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelCollider wheel = wheels[i].GetComponent<WheelCollider>();

            wheel.steerAngle = (i < 2) ? currentTurnAngle : 0f;

            wheel.motorTorque = (i >= 2) ? v * enginePower : 0f;

            wheel.brakeTorque = Mathf.Abs(v) < 0.1f ? brakePower : 0f;

            if (i < wheelMeshes.Length)
            {
                wheel.GetWorldPose(out Vector3 pos, out Quaternion rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }
    }
}
