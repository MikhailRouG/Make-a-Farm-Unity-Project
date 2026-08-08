using System;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Player
{
    public class PlayerInteraction : NetworkBehaviour
    {
        private const float DistanceTolerance = 1.5f;

        [Header("References")]
        [SerializeField] private Transform _rayOrigin;

        [Header("Settings")]
        [SerializeField] private float _interactDistance = 3f;
        [SerializeField] private LayerMask _interactableMask;

        private Camera _playerCamera;
        private IInteractable _currentTarget;

        public bool HasTarget { get; private set; }
        public Vector3 LookPoint { get; private set; }

        public event Action<string> OnHasInteraction;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (_playerCamera == null)
                _playerCamera = FindAnyObjectByType<Camera>();

            if (_rayOrigin == null)
                _rayOrigin = transform;
        }

        public void InteractionByForward()
        {
            if (!isLocalPlayer || _playerCamera == null) return;

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            Interaction(ray);
        }

        public void InteractionByCursor()
        {
            if (!isLocalPlayer || _playerCamera == null) return;

            Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
            Interaction(ray);
        }

        public void Interaction(Ray ray)
        {
            if (CheckMouseOnUi()) return;

            Debug.DrawRay(ray.origin, ray.direction * _interactDistance, Color.green);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactableMask))
            {
                LookPoint = hit.point;

                if (hit.collider.TryGetComponent(out IInteractable component))
                {
                    if (_currentTarget != component)
                    {
                        _currentTarget = component;
                        HasTarget = true;
                        OnHasInteraction?.Invoke(_currentTarget.InteractionPrompt);
                    }

                    return;
                }
            }

            ClearTarget();
        }

        public void TryInteract()
        {
            if (!isLocalPlayer || _currentTarget == null) return;

            if (_currentTarget is UnityEngine.Object unityObj && unityObj == null)
            {
                _currentTarget = null;
                return;
            }

            if (_currentTarget is Component component)
            {
                if (component.TryGetComponent(out NetworkIdentity identity))
                    CmdExecuteInteraction(identity);
                else
                    _currentTarget.Interact(gameObject);
            }

            _currentTarget = null;
        }

        // The target comes from the client, so the distance is re-checked here.
        // The tolerance absorbs the position drift between client and server.
        [Command]
        private void CmdExecuteInteraction(NetworkIdentity targetIdentity)
        {
            if (targetIdentity == null)
                return;

            float distance = Vector3.Distance(transform.position, targetIdentity.transform.position);

            if (distance > _interactDistance * DistanceTolerance)
                return;

            if (targetIdentity.TryGetComponent(out IInteractable serverTarget))
                serverTarget.Interact(gameObject);
        }

        private void ClearTarget()
        {
            if (HasTarget)
                OnHasInteraction?.Invoke(string.Empty);

            _currentTarget = null;
            HasTarget = false;
        }

        private bool CheckMouseOnUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearTarget();
                return true;
            }

            return false;
        }
    }
}
