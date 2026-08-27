using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private PlayerCameraRig _cameraRig;
        [SerializeField] private GameObject[] _playerModel;
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float gamepadSensitivity = 180f;
        [SerializeField] private float _zoomSpeed = 50f;
        [SerializeField] private float clampAngle = 80f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 5f;
        [SerializeField] private bool _firstPerson;

        private Transform _rigTransform;
        private float xRotation;
        private float yRotation;
        private bool _initialized;

        public bool FirstPerson => _firstPerson;
        public Transform AimTransform => _rigTransform;

        private void Awake()
        {
            if (_cameraRig == null) _cameraRig = GetComponentInChildren<PlayerCameraRig>(true);
            if (_cameraRig == null)
            {
                Debug.LogError($"[{nameof(PlayerCameraController)}] Camera rig is not assigned.", this);
                return;
            }

            _rigTransform = _cameraRig.transform;

            // The rig ships inside the player prefab, so every spawned player owns one.
            // Off until Initialize: only the local player's rig may drive the brain,
            // otherwise remote players fight over it for the same output channel.
            _cameraRig.gameObject.SetActive(false);
        }

        public void Initialize()
        {
            if (_cameraRig == null) return;

            _cameraRig.gameObject.SetActive(true);

            Vector3 currentRotation = _rigTransform.rotation.eulerAngles;
            xRotation = NormalizeAngle(currentRotation.x);
            yRotation = NormalizeAngle(currentRotation.y);

            _initialized = true;

            ApplyRotation();
            ChangeCamera(_firstPerson);
        }

        // Re-applied every frame, not only on look input: the rig is a child of the
        // player and the body turns to follow the camera, so an inherited rotation
        // would feed that turn back into the look direction and drift.
        private void LateUpdate()
        {
            if (!_initialized) return;

            ApplyRotation();
        }

        public void HandleRotate(Vector2 look, bool isPointerDelta = true)
        {
            if (!_initialized) return;
            if (_firstPerson && Cursor.visible) return;

            float scale = isPointerDelta
                ? mouseSensitivity * 0.01f
                : gamepadSensitivity * Time.deltaTime;

            xRotation -= look.y * scale;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            yRotation += look.x * scale;

            ApplyRotation();
        }

        public void HandleZoom(float zoom)
        {
            if (!_initialized) return;

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

        // World rotation, not local: the pivot follows the player's position through
        // the hierarchy, but its aim must stay independent of the body's yaw.
        private void ApplyRotation()
        {
            _rigTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
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
