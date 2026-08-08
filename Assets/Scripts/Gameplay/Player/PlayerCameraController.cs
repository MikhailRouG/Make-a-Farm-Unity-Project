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
        [SerializeField] private float gamepadSensitivity = 180f;
        [SerializeField] private float _zoomSpeed = 50f;
        [SerializeField] private float clampAngle = 80f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 5f;
        [SerializeField] private bool _firstPerson;

        private const float MouseDeltaScale = 1f / 140f;

        private PlayerCameraRig _cameraRig;
        private Transform _cameraTrackTransform;
        private float xRotation;
        private float yRotation;

        public bool FirstPerson => _firstPerson;
        public Transform AimTransform => _cameraTrackTransform;

        public void Initialize()
        {
            if (_cameraPrefab == null || _cameraTarget == null)
            {
                Debug.LogError($"[{nameof(PlayerCameraController)}] Camera prefab or target is not assigned.", this);
                return;
            }

            Vector3 currentRotation = _cameraTarget.localEulerAngles;
            xRotation = NormalizeAngle(currentRotation.x);
            yRotation = NormalizeAngle(currentRotation.y);

            _cameraRig = Instantiate(_cameraPrefab);
            _cameraTrackTransform = _cameraRig.transform;

            ChangeCamera(_firstPerson);
        }

        private void LateUpdate()
        {
            if (_cameraTrackTransform == null || _cameraTarget == null) return;

            _cameraTrackTransform.position = _cameraTarget.position;
        }

        public void HandleRotate(Vector2 look, bool isPointerDelta = true)
        {
            if (_cameraTarget == null || _cameraTrackTransform == null) return;
            if (_firstPerson && Cursor.visible) return;

            float scale = isPointerDelta
                ? mouseSensitivity * MouseDeltaScale
                : gamepadSensitivity * Time.deltaTime;

            xRotation -= look.y * scale;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            yRotation += look.x * scale;

            _cameraTrackTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        public void HandleZoom(float zoom)
        {
            if (_cameraRig == null) return;

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
            if (_playerModel != null)
            {
                foreach (GameObject model in _playerModel)
                {
                    if (model != null)
                        model.SetActive(!firstCamera);
                }
            }

            _firstPerson = firstCamera;
            _cameraRig.SetFirstPerson(firstCamera);
            Cursor.visible = !firstCamera;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}