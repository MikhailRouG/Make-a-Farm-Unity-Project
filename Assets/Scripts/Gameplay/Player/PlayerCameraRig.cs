using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Player
{
    public sealed class PlayerCameraRig : MonoBehaviour
    {
        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera _firstPersonCamera;
        [SerializeField] private CinemachineCamera _thirdPersonCamera;

        [SerializeField]
        private CinemachineThirdPersonFollow _thirdPersonFollow;

        public CinemachineCamera FirstPersonCamera => _firstPersonCamera;
        public CinemachineCamera ThirdPersonCamera => _thirdPersonCamera;
        public CinemachineThirdPersonFollow ThirdPersonFollow =>
            _thirdPersonFollow;

        public void SetFirstPerson(bool firstPersonEnabled)
        {
            _firstPersonCamera.enabled = firstPersonEnabled;
            _thirdPersonCamera.enabled = !firstPersonEnabled;
        }
    }
}