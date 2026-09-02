using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        private Transform _rig;

        // The local player's own body: the camera sits inside the head, so it is
        // hidden for the owner and left visible on every other client.
        [SerializeField] private GameObject[] _playerModel;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float gamepadSensitivity = 180f;
        [SerializeField] private float clampAngle = 80f;

        private float xRotation;
        private float yRotation;
        private bool _initialized;

        public Transform AimTransform => _rig;

        private void Awake()
        {
            _rig = GetComponentInChildren<CinemachineCamera>().transform;
            if (_rig == null)
            {
                Debug.LogError($"[{nameof(PlayerCameraController)}] Camera rig is not assigned.", this);
                return;
            }

            // The rig ships inside the player prefab, so every spawned player owns one.
            _rig.gameObject.SetActive(false);
        }

        public void Initialize()
        {
            if (_rig == null) return;

            _rig.gameObject.SetActive(true);

            Vector3 currentRotation = _rig.rotation.eulerAngles;
            xRotation = NormalizeAngle(currentRotation.x);
            yRotation = NormalizeAngle(currentRotation.y);

            _initialized = true;

            ApplyRotation();
            HideOwnModel();
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

            // A visible cursor means the mouse belongs to the world cursor - planting,
            // interaction - and must not turn the head at the same time.
            if (Cursor.visible) return;

            float scale = isPointerDelta
                ? mouseSensitivity * 0.01f
                : gamepadSensitivity * Time.deltaTime;

            xRotation -= look.y * scale;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            yRotation += look.x * scale;

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            _rig.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        private void HideOwnModel()
        {
            if (_playerModel == null) return;

            foreach (GameObject model in _playerModel)
            {
                if (model != null)
                    model.SetActive(false);
            }
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
