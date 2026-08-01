using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private PlayerCameraRig _cameraPrefab;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private GameObject[] _playerModel;
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float _zoomSpeed = 50f;
        [SerializeField] private float clampAngle = 80f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 5f;
        [SerializeField] private bool _firstPerson;
        public bool FirstPerson => _firstPerson;
        private PlayerCameraRig _cameraRig;
        private Transform _cameraTrackTransform;
        private float xRotation;
        private float yRotation;
        public void Initialize()
        {
            Vector3 currentRotation = _cameraTarget.localEulerAngles;
            xRotation = NormalizeAngle(currentRotation.x);
            yRotation = NormalizeAngle(currentRotation.y);
            var cam = Instantiate(_cameraPrefab);
            _cameraTrackTransform = cam.gameObject.transform;
            _cameraRig = cam;
            ChangeCamera(_firstPerson);
        }
        private void LateUpdate()
        {
            if (_cameraRig != null)
            {
                _cameraTrackTransform.position = _cameraTarget.position;
            }
        }
        public void HandleRotate(Vector2 look)
        {
            float mouseX = look.x * mouseSensitivity * Time.deltaTime;
            float mouseY = look.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            yRotation += mouseX;

            if (_cameraTarget != null)
                _cameraTrackTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        }
        public void HandleZoom(float zoom)
        {
            if (_cameraRig.FirstPersonCamera.enabled)
            {
                if (zoom < 0)
                {
                    ChangeCamera(false);
                }
                return;
            }

            float currentDistance = _cameraRig.ThirdPersonFollow.CameraDistance;

            float newDistance = currentDistance - zoom * _zoomSpeed * Time.deltaTime;
            if (newDistance <= _minDistance)
            {
                _cameraRig.ThirdPersonFollow.CameraDistance = _minDistance;
                ChangeCamera(true);
                return;
            }
            newDistance = Mathf.Clamp(newDistance, _minDistance, _maxDistance);
            _cameraRig.ThirdPersonFollow.CameraDistance = newDistance;
        }
        private void ChangeCamera(bool firstCamera)
        {
            foreach(GameObject model in _playerModel)
                model.SetActive(!firstCamera);
            _firstPerson = firstCamera;
            _cameraRig.SetFirstPerson(firstCamera);
        }
        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}