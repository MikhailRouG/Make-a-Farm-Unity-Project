using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private GameObject _cameraPrefab;
    [SerializeField] private Transform _pointTransform;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float _zoomSpeed = 50f;
    [SerializeField] private float clampAngle = 80f;

    private CinemachineThirdPersonFollow cameraSetting;
    private Transform _cameraTrackTransform;
    private float xRotation;
    private float yRotation;
    public void Initialize()
    {
        xRotation = 0;
        yRotation = 0;
        var cam = Instantiate(_cameraPrefab);
        _cameraTrackTransform = cam.transform;
            cameraSetting = cam.GetComponentInChildren<CinemachineThirdPersonFollow>(); 
            if (cameraSetting == null ) 
            { 
            Debug.LogError($"[{gameObject.name}] CinemachineThirdPersonFollow! = null");
            } 
    }
    private void LateUpdate()
    {
        if (cameraSetting != null)
        {
            _cameraTrackTransform.position = _pointTransform.position;
        }
    }
    public void HandleCamera(Vector2 look, float zoom)
    {
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        yRotation += mouseX;

        if (_cameraTrackTransform != null)
            _cameraTrackTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (zoom != 0)
        {
            cameraSetting.CameraDistance -= zoom * _zoomSpeed * Time.deltaTime;
            cameraSetting.CameraDistance = Mathf.Clamp(cameraSetting.CameraDistance, 2, 15);
        }
    }
}
